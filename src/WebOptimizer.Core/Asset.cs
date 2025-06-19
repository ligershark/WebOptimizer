using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer;

/// <summary>
/// Extension methods for <see cref="IAssetPipeline"/>.
/// </summary>
public static class AssetExtensions
{
    /// <summary>
    /// Adds a file name pattern for files that should be excluded from the results
    /// </summary>
    public static IAsset ExcludeFiles(this IAsset asset, params string[] filesToExclude)
    {
        if (filesToExclude.Length == 0)
        {
            throw new ArgumentException("At least one file has to be specified", nameof(filesToExclude));
        }

        foreach (string file in filesToExclude)
        {
            asset.ExcludeFiles.Add(file);
        }

        return asset;
    }

    /// <summary>
    /// Gets the asset file provider. This method works for _content locations in RCL projects.
    /// </summary>
    public static IFileProvider GetAssetFileProvider(this IAsset asset, IWebHostEnvironment env)
    {
        return asset.GetCustomFileProvider(env) ??
               env.WebRootFileProvider as CompositeFileProvider ?? env.WebRootFileProvider;
    }

    /// <summary>
    /// Gets the file provider.
    /// </summary>
    public static IFileProvider GetFileProvider(this IAsset asset, IWebHostEnvironment env)
    {
        return asset.GetCustomFileProvider(env) ??
               (env.WebRootFileProvider is CompositeFileProvider provider
                   ? provider.FileProviders.Last()
                   : env.WebRootFileProvider);
    }

    internal static IEnumerable<IAsset> AddProcessor(this IEnumerable<IAsset> assets, Func<IAsset, IAsset> processor)
    {
        var list = new List<IAsset>();

        foreach (var asset in assets)
        {
            list.Add(processor(asset));
        }

        return list;
    }

    /// <summary>
    /// Returns all files from the file provider, beginning with <paramref name="start"/>
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    internal static IReadOnlyList<string> GetAllFiles(this IFileProvider provider, string start)
    {
        var files = new List<string>();
        var dirs = new Queue<string>();

        var infos = provider.GetDirectoryContents(start) ?? NotFoundDirectoryContents.Singleton;

        foreach (var info in infos)
        {
            if (info.IsDirectory)
            {
                dirs.Enqueue(info.Name);
            }
            else if (info.Exists)
            {
                files.Add(info.Name);
            }
        }

        while (dirs.Count > 0)
        {
            string path = dirs.Dequeue();

            infos = provider.GetDirectoryContents(path) ?? NotFoundDirectoryContents.Singleton;

            foreach (var info in infos)
            {
                if (info.IsDirectory)
                {
                    dirs.Enqueue(Path.Combine(path, info.Name));
                }
                else if (info.Exists)
                {
                    files.Add(Path.Combine(path, info.Name));
                }
            }
        }

        return files;
    }

    /// <summary>
    /// Gets the file provider.
    /// </summary>
    internal static IFileProvider GetFileProvider(this IAsset asset, IWebHostEnvironment env, string path, out string outpath)
    {
        var provider = asset.GetCustomFileProvider(env) ?? env.WebRootFileProvider;

        if (provider is CompositeFileProviderExtended extended)
        {
            return extended.GetFileProvider(path, out outpath);
        }

        outpath = path;
        return provider;
    }
}

internal class Asset(string route, string contentType, IEnumerable<string> sourceFiles, ILogger<Asset> logger) : IAsset
{
    internal const string PhysicalFilesKey = "PhysicalFiles";
    private readonly ILogger<Asset> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lock _sync = new();

    public string ContentType { get; private set; } = contentType ?? throw new ArgumentNullException(nameof(contentType));
    public IList<string> ExcludeFiles { get; } = [];
    public IDictionary<string, object> Items { get; } = new ConcurrentDictionary<string, object>();
    public IList<IProcessor> Processors { get; } = [];
    public string Route { get; private set; } = route ?? throw new ArgumentNullException(nameof(route));
    public IList<string> SourceFiles { get; } = sourceFiles?.ToList() ?? throw new ArgumentNullException(nameof(sourceFiles));

    public static IEnumerable<string> ExpandGlobs(IAsset asset, IWebHostEnvironment env)
    {
        var files = new List<string>();

        if (asset.SourceFiles.Any())
        {
            foreach (string sourceFile in asset.SourceFiles)
            {
                var provider = asset.GetFileProvider(env, sourceFile, out string outSourceFile);

                if (asset.ExcludeFiles.Count == 0 && provider.GetFileInfo(outSourceFile).Exists)
                {
                    if (!files.Contains(sourceFile))
                    {
                        files.Add(sourceFile);
                    }
                }
                else
                {
                    var virtualFilePaths = provider.GetAllFiles("/");

                    var matcher = new Matcher();
                    _ = matcher.AddInclude(outSourceFile);
                    matcher.AddExcludePatterns(asset.ExcludeFiles);
                    var globbingResult = matcher.Match(virtualFilePaths);

                    var fileMatches = globbingResult.Files.Select(f => f.Path);

                    bool sourceIsRooted = outSourceFile.StartsWith('/');
                    if (sourceIsRooted)
                    {
                        fileMatches = fileMatches.Select(f => "/" + f);
                    }

                    if (!fileMatches.Any())
                    {
                        continue;
                    }

                    files.AddRange(fileMatches.Where(f => !files.Contains(f)));
                }
            }

            if (files.Count == 0)
            {
                throw new FileNotFoundException($"No files found matching exist in an asset {asset}");
            }

            asset.Items[PhysicalFilesKey] = files;
        }

        return files;
    }

    public async Task<byte[]> ExecuteAsync(HttpContext context, IWebOptimizerOptions options)
    {
        var env = (IWebHostEnvironment)context.RequestServices.GetRequiredService(typeof(IWebHostEnvironment));
        var config = new AssetContext(context, this, options);

        var files = ExpandGlobs(this, env);

        var lastModified = DateTime.MinValue;

        // Read file content into memory
        foreach (string file in files)
        {
            if (!config.Content.ContainsKey(file))
            {
                var dateChanged = await LoadFileContentAsync(this.GetAssetFileProvider(env), config, file);

                if (dateChanged > lastModified)
                {
                    lastModified = dateChanged;
                }
            }
        }

        if (lastModified != DateTime.MinValue)
        {
            context.Response.Headers[HeaderNames.LastModified] = lastModified.ToString("R");
        }

        // Attach the processors
        foreach (var processor in Processors)
        {
            await processor.ExecuteAsync(config).ConfigureAwait(false);
        }

        return config.Content.FirstOrDefault().Value;
    }

    public string GenerateCacheKey(HttpContext context, IWebOptimizerOptions options)
    {
        var config = new AssetContext(context, this, options);

        var cacheKey = new StringBuilder(Route);

        if (context.Request.Headers.TryGetValue("Accept-Encoding", out var enc))
        {
            _ = cacheKey.Append(enc.ToString());
        }

        IEnumerable<string>? physicalFiles;
        var env = (IWebHostEnvironment)context.RequestServices.GetRequiredService(typeof(IWebHostEnvironment));
        var cache = (IMemoryCache)context.RequestServices.GetRequiredService(typeof(IMemoryCache));

        var fileVersionProvider = new FileVersionProvider(
            this.GetAssetFileProvider(env),
            cache,
            context.Request.PathBase);

        physicalFiles = Items.TryGetValue(PhysicalFilesKey, out object? value) ? value as IEnumerable<string> : ExpandGlobs(this, env);

        if (physicalFiles is not null)
        {
            foreach (string file in physicalFiles)
            {
                _ = cacheKey.Append(fileVersionProvider.AddFileVersionToPath(file));
            }
        }

        foreach (var processors in Processors)
        {
            try
            {
                _ = cacheKey.Append(processors.CacheKey(context, config) ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception($"CacheKey generation exception in {processors.GetType().FullName} processor", ex);
            }
        }
        byte[] buffer = Encoding.UTF8.GetBytes(cacheKey.ToString());
        byte[] hash = SHA256.HashData(buffer);
        return WebEncoders.Base64UrlEncode(hash);
    }

    public override string ToString()
    {
        return Route;
    }

    /// <summary>
    /// Adds a source file to the asset
    /// </summary>
    /// <param name="route">Relative path of a source file</param>
    public void TryAddSourceFile(string route)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new ArgumentNullException(nameof(route));
        }

        string cleanRoute = route.TrimStart('~');

        lock (_sync)
        {
            if (SourceFiles.Contains(cleanRoute))
            {
                _logger.LogSourceFileAlreadyAdded(route, cleanRoute);
            }

            if (SourceFiles.Contains(cleanRoute) && Items.ContainsKey(PhysicalFilesKey))
            {
                _ = Items.Remove(Asset.PhysicalFilesKey); //remove to calc a new cache key
            }
        }
    }

    private static async Task<DateTime> LoadFileContentAsync(IFileProvider fileProvider, AssetContext config, string sourceFile)
    {
        var file = fileProvider.GetFileInfo(sourceFile);

        await using var fs = file.CreateReadStream();
        byte[] bytes = await fs.AsBytesAsync();

        if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            bytes = [.. bytes.Skip(3)];
        }

        config.Content.Add(sourceFile, bytes);

        return file.LastModified.UtcDateTime;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer
{
    internal class Asset : IAsset
    {
        internal const string PhysicalFilesKey = "PhysicalFiles";
        private readonly object _sync = new object();

        public Asset(string route, string contentType, IAssetPipeline pipeline, IEnumerable<string> sourceFiles)
            : this(route, contentType, sourceFiles)
        { }

        public Asset(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            SourceFiles = new HashSet<string>(sourceFiles ?? throw new ArgumentNullException(nameof(sourceFiles)));
            Processors = new List<IProcessor>();
            Items = new Dictionary<string, object>();
        }

        public string Route { get; private set; }

        public IList<string> ExcludeFiles { get; } = new List<string>();

        public HashSet<string> SourceFiles { get; }

        public string ContentType { get; private set; }

        public IList<IProcessor> Processors { get; }

        public IDictionary<string, object> Items { get; }

        public async Task<byte[]> ExecuteAsync(HttpContext context, IWebOptimizerOptions options)
        {
            var env = (IWebHostEnvironment)context.RequestServices.GetService(typeof(IWebHostEnvironment));
            var config = new AssetContext(context, this, options);

            IEnumerable<string> files = ExpandGlobs(this, env);

            DateTime lastModified = DateTime.MinValue;

            // Read file content into memory
            foreach (string file in files)
            {
                if (!config.Content.ContainsKey(file))
                {
                    DateTime dateChanged = await LoadFileContentAsync(this.GetFileProvider(env), config, file);

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
            foreach (IProcessor processor in Processors)
            {
                await processor.ExecuteAsync(config).ConfigureAwait(false);
            }

            return config.Content.FirstOrDefault().Value;
        }

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
                        matcher.AddInclude(outSourceFile);
                        matcher.AddExcludePatterns(asset.ExcludeFiles);
                        PatternMatchingResult globbingResult = matcher.Match(virtualFilePaths);

                        IEnumerable<string> fileMatches = globbingResult.Files.Select(f => f.Path);

                        var sourceIsRooted = outSourceFile.StartsWith('/');
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
                    throw new FileNotFoundException($"No files found matching exist in an asset");
                }

                asset.Items[PhysicalFilesKey] = files;
            }

            return files;
        }

        private static async Task<DateTime> LoadFileContentAsync(IFileProvider fileProvider, AssetContext config, string sourceFile)
        {
            IFileInfo file = fileProvider.GetFileInfo(sourceFile);

            using (Stream fs = file.CreateReadStream())
            {
                byte[] bytes = await fs.AsBytesAsync();

                if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                {
                    bytes = bytes.Skip(3).ToArray();
                }

                config.Content.Add(sourceFile, bytes);
            }

            return file.LastModified.UtcDateTime;
        }

        public string GenerateCacheKey(HttpContext context, IWebOptimizerOptions options)
        {
            var config = new AssetContext(context, this, options);

            var cacheKey = new StringBuilder(Route);

            if (context.Request.Headers.TryGetValue("Accept-Encoding", out StringValues enc))
            {
                cacheKey.Append(enc.ToString());
            }

            IEnumerable<string> physicalFiles;
            var env = (IWebHostEnvironment)context.RequestServices.GetService(typeof(IWebHostEnvironment));
            var cache = (IMemoryCache)context.RequestServices.GetService(typeof(IMemoryCache));

            var fileVersionProvider = new FileVersionProvider(
                this.GetFileProvider(env),
                cache,
                context.Request.PathBase);

            if (!Items.ContainsKey(PhysicalFilesKey))
            {
                physicalFiles = ExpandGlobs(this, env);
            }
            else
            {
                physicalFiles = Items[PhysicalFilesKey] as IEnumerable<string>;
            }

            if (physicalFiles != null)
            {
                foreach (string file in physicalFiles)
                {
                    cacheKey.Append(fileVersionProvider.AddFileVersionToPath(file));
                }
            }

            foreach (IProcessor processors in Processors)
            {
                try
                {
                    cacheKey.Append(processors.CacheKey(context, config) ?? string.Empty);
                }
                catch (Exception ex)
                {
                    throw new Exception($"CacheKey generation exception in {processors.GetType().FullName} processor", ex);
                }
            }

            using (var algo = SHA1.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(cacheKey.ToString());
                byte[] hash = algo.ComputeHash(buffer);
                return WebEncoders.Base64UrlEncode(hash);
            }
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
                throw new ArgumentNullException(nameof(route));

            string cleanRoute = route.TrimStart('~');

            lock (_sync)
            {
                if (SourceFiles.Add(cleanRoute) && Items.ContainsKey(PhysicalFilesKey))
                    Items.Remove(Asset.PhysicalFilesKey); //remove to calc a new cache key
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class AssetExtensions
    {
        internal static IEnumerable<IAsset> AddProcessor(this IEnumerable<IAsset> assets, Func<IAsset, IAsset> processor)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(processor(asset));
            }

            return list;
        }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        public static IFileProvider GetFileProvider(this IAsset asset, IWebHostEnvironment env)
        {
            return asset.GetCustomFileProvider(env) ??
                   (env.WebRootFileProvider is CompositeFileProvider
                       ? (env.WebRootFileProvider as CompositeFileProvider).FileProviders.Last()
                       : env.WebRootFileProvider);
        }

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
                asset.ExcludeFiles.Add(file);

            return asset;
        }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        internal static IFileProvider GetFileProvider(this IAsset asset, IWebHostEnvironment env, string path, out string outpath)
        {
            var provider = asset.GetCustomFileProvider(env) ?? env.WebRootFileProvider;

            if (provider is CompositeFileProviderExtended)
            {
                return ((CompositeFileProviderExtended)provider).GetFileProvider(path, out outpath);
            }

            outpath = path;
            return provider;
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

            var infos = provider.GetDirectoryContents(start);

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
                var path = dirs.Dequeue();

                infos = provider.GetDirectoryContents(path);

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
    }
}

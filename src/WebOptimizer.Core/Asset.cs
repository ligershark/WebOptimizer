using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer
{
    internal class Asset : IAsset
    {
        private static FileVersionProvider _fileVersionProvider;
        internal const string PhysicalFilesKey = "PhysicalFiles";

        public Asset(string route, string contentType, IAssetPipeline pipeline, IEnumerable<string> sourceFiles)
            : this(route, contentType, sourceFiles)
        { }

        public Asset(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            SourceFiles = sourceFiles ?? throw new ArgumentNullException(nameof(sourceFiles));
            Processors = new List<IProcessor>();
            Items = new Dictionary<string, object>();
        }

        public string Route { get; private set; }

        public IEnumerable<string> SourceFiles { get; internal set; }

        public string ContentType { get; private set; }

        public IList<IProcessor> Processors { get; }

        public IDictionary<string, object> Items { get; }


        public async Task<byte[]> ExecuteAsync(HttpContext context, IWebOptimizerOptions options)
        {
            var env = (IHostingEnvironment)context.RequestServices.GetService(typeof(IHostingEnvironment));
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

        public static IEnumerable<string> ExpandGlobs(IAsset asset, IHostingEnvironment env)
        {
            var files = new List<string>();

            foreach (string sourceFile in asset.SourceFiles)
            {
                string outSourceFile;
                var provider = asset.GetFileProvider(env, sourceFile, out outSourceFile);

                var fileInfo = provider.GetFileInfo("/");
                string root = fileInfo.PhysicalPath;
                if (root != null)
                {
                    var dir = new DirectoryInfoWrapper(new DirectoryInfo(root));
                    var matcher = new Matcher();
                    matcher.AddInclude(outSourceFile);
                    PatternMatchingResult globbingResult = matcher.Execute(dir);
                    IEnumerable<string> fileMatches = globbingResult.Files.Select(f => f.Path.Replace(root, string.Empty));

                    if (!fileMatches.Any())
                    {
                        throw new FileNotFoundException($"No files found matching \"{sourceFile}\" exist in \"{dir.FullName}\"");
                    }
                    //files.AddRange(fileMatches.Where(f => !files.Contains(f)));
                    files.Add(sourceFile);
                }
                else
                {
                    files.Add(sourceFile);
                }


            }

            asset.Items[PhysicalFilesKey] = files;

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

        public string GenerateCacheKey(HttpContext context)
        {
            var cacheKey = new StringBuilder(Route);

            if (context.Request.Headers.TryGetValue("Accept-Encoding", out StringValues enc))
            {
                cacheKey.Append(enc.ToString());
            }

            IEnumerable<string> physicalFiles;
            var env = (IHostingEnvironment)context.RequestServices.GetService(typeof(IHostingEnvironment));

            if (_fileVersionProvider == null)
            {
                var cache = (IMemoryCache)context.RequestServices.GetService(typeof(IMemoryCache));

                _fileVersionProvider = new FileVersionProvider(
                    this.GetFileProvider(env),
                    cache,
                    context.Request.PathBase);
            }

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
                    cacheKey.Append(_fileVersionProvider.AddFileVersionToPath(file));
                }
            }

            foreach (IProcessor processors in Processors)
            {
                try
                {
                    cacheKey.Append(processors.CacheKey(context) ?? string.Empty);
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
        public static IFileProvider GetFileProvider(this IAsset asset, IHostingEnvironment env)
        {
            return asset.GetCustomFileProvider(env) ?? env.WebRootFileProvider;
        }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        internal static IFileProvider GetFileProvider(this IAsset asset, IHostingEnvironment env, string path, out string outpath)
        {
            var provider = asset.GetCustomFileProvider(env) ?? env.WebRootFileProvider;

            if (provider is CompositeFileProviderExtended)
            {
                return ((CompositeFileProviderExtended)provider).GetFileProvider(path, out outpath);
            }

            outpath = path;
            return provider;
        }
    }
}

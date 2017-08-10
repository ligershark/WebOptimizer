using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace WebOptimizer
{
    internal class Asset : IAsset
    {
        public Asset(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            SourceFiles = sourceFiles ?? throw new ArgumentNullException(nameof(sourceFiles));
            Processors = new List<IProcessor>();
        }

        public string Route { get; private set; }

        public IEnumerable<string> SourceFiles { get; internal set; }

        public string ContentType { get; private set; }

        public IList<IProcessor> Processors { get; }

        public async Task<byte[]> ExecuteAsync(HttpContext context, WebOptimizerOptions options)
        {
            string root = options.FileProvider.GetFileInfo("/").PhysicalPath;
            var config = new AssetContext(context, this);

            // Handle globbing
            var dir = new DirectoryInfoWrapper(new DirectoryInfo(root));
            var matcher = new Matcher();
            matcher.AddIncludePatterns(SourceFiles);
            PatternMatchingResult globbingResult = matcher.Execute(dir);
            IEnumerable<string> files = globbingResult.Files.Select(f => f.Path.Replace(root, string.Empty));

            // Read file content into memory
            foreach (string file in files)
            {
                if (!config.Content.ContainsKey(file))
                {
                    await LoadFileContentAsync(options.FileProvider, config, file);
                }
            }

            // Attach the processors
            foreach (IProcessor processor in Processors)
            {
                await processor.ExecuteAsync(config, options).ConfigureAwait(false);
            }

            return config.Content.FirstOrDefault().Value;
        }

        private static async Task LoadFileContentAsync(IFileProvider fileProvider, AssetContext config, string sourceFile)
        {
            IFileInfo file = fileProvider.GetFileInfo(sourceFile);

            using (Stream fs = file.CreateReadStream())
            {
                byte[] bytes = await fs.AsBytesAsync();
                config.Content.Add(sourceFile, bytes);
            }
        }

        public string GenerateCacheKey(HttpContext context)
        {
            string cacheKey = Route;

            if (context.Request.Headers.TryGetValue("Accept-Encoding", out var enc))
            {
                cacheKey += enc.ToString();
            }

            foreach (IProcessor processors in Processors)
            {
                cacheKey += processors.CacheKey(context) ?? string.Empty;
            }

            using (var algo = SHA1.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(cacheKey);
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
    internal static class AssetExtensions
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
    }
}

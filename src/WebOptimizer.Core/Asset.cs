using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace WebOptimizer
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    internal class Asset : IAsset
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Asset"/> class.
        /// </summary>
        protected Asset()
        { }

        /// <summary>
        /// Gets the route to the bundle output.
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        public IEnumerable<string> SourceFiles { get; internal set; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        public IList<IProcessor> Processors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        public static IAsset Create(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            if (!route.StartsWith("/") && !route.StartsWith("."))
            {
                route = "/" + route;
            }

            return new Asset
            {
                Route = route,
                ContentType = contentType,
                SourceFiles = sourceFiles,
                Processors = new List<IProcessor>(),
            };
        }

        /// <summary>
        /// Executes the bundle and returns the processed output.
        /// </summary>
        public async Task<byte[]> ExecuteAsync(HttpContext context)
        {
            var pipeline = (IAssetPipeline)context.RequestServices.GetService(typeof(IAssetPipeline));
            string root = pipeline.FileProvider.GetFileInfo("/").PhysicalPath;
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
                    await LoadFileContentAsync(pipeline, config, file);
                }
            }

            // Attach the processors
            foreach (IProcessor processor in Processors)
            {
                await processor.ExecuteAsync(config).ConfigureAwait(false);
            }

            return config.Content.FirstOrDefault().Value;
        }

        private static async Task LoadFileContentAsync(IAssetPipeline pipeline, AssetContext config, string sourceFile)
        {
            IFileInfo file = pipeline.FileProvider.GetFileInfo(sourceFile);

            if (file.Exists)
            {
                using (Stream fs = file.CreateReadStream())
                {
                    byte[] bytes = await fs.AsBytesAsync();
                    config.Content.Add(sourceFile, bytes);
                }
            }
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        public string GenerateCacheKey(HttpContext context)
        {
            string cacheKey = Route;

            if (context.Request.Headers.TryGetValue("Accept-Encoding", out var enc))
            {
                cacheKey += enc.ToString();
            }

            foreach (IProcessor processors in Processors)
            {
                cacheKey += processors.CacheKey(context);
            }

            using (var algo = SHA1.Create())
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(cacheKey);
                byte[] hash = algo.ComputeHash(buffer);
                return WebEncoders.Base64UrlEncode(hash);
            }
        }

        public override string ToString()
        {
            return Route;
        }
    }
}

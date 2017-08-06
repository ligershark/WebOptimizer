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
    internal class Asset : IAsset
    {
        private Asset()
        { }

        public string Route { get; private set; }

        public IEnumerable<string> SourceFiles { get; internal set; }

        public string ContentType { get; private set; }

        public IList<IProcessor> Processors { get; private set; }

        public static IAsset Create(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            return new Asset
            {
                Route = $"/{route.TrimStart('/')}",
                ContentType = contentType,
                SourceFiles = sourceFiles,
                Processors = new List<IProcessor>(),
            };
        }

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

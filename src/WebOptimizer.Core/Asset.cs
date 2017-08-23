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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Net.Http.Headers;

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
            string root = this.GetFileProvider(env).GetFileInfo("/").PhysicalPath;
            var config = new AssetContext(context, this, options);

            // Handle globbing
            var dir = new DirectoryInfoWrapper(new DirectoryInfo(root));
            var files = new List<string>();

            foreach (string sourceFile in SourceFiles)
            {
                var matcher = new Matcher();
                matcher.AddInclude(sourceFile);
                PatternMatchingResult globbingResult = matcher.Execute(dir);
                IEnumerable<string> fileMatches = globbingResult.Files.Select(f => f.Path.Replace(root, string.Empty));

                if (!fileMatches.Any())
                {
                    throw new FileNotFoundException($"No files found matching \"{sourceFile}\" exist in \"{dir.FullName}\"");
                }

                files.AddRange(fileMatches.Where(f => !files.Contains(f)));
            }

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

        private static async Task<DateTime> LoadFileContentAsync(IFileProvider fileProvider, AssetContext config, string sourceFile)
        {
            IFileInfo file = fileProvider.GetFileInfo(sourceFile);

            using (Stream fs = file.CreateReadStream())
            {
                byte[] bytes = await fs.AsBytesAsync();
                config.Content.Add(sourceFile, bytes);
            }

            return file.LastModified.UtcDateTime;
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
            return asset.IsUsingContentRoot() ? env.ContentRootFileProvider : env.WebRootFileProvider;
        }
    }
}

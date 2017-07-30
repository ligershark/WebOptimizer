using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Bundler
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
            var bundle = new Asset
            {
                Route = route,
                ContentType = contentType,
                SourceFiles = sourceFiles,
                Processors = new List<IProcessor>(),
            };

            return bundle;
        }

        /// <summary>
        /// Executes the bundle and returns the processed output.
        /// </summary>
        public async Task<string> ExecuteAsync(HttpContext context)
        {
            var config = new AssetContext(context, this);

            foreach (IProcessor processor in Processors)
            {
                await processor.ExecuteAsync(config).ConfigureAwait(false);
            }

            return config.Content;
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
    }
}

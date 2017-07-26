using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Bundler
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    public class BundleMiddleware : BaseMiddleware
    {
        private readonly Bundle _bundle;

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BundleMiddleware(RequestDelegate next, IHostingEnvironment env, Bundle bundle, IMemoryCache cache)
            : base(next, cache, env)
        {
            _bundle = bundle;
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected override string ContentType => _bundle.ContentType;

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public override async Task<string> ExecuteAsync(HttpContext context)
        {
            string source = await GetContentAsync(_bundle);

            var config = new BundlerProcess(context, _bundle)
            {
                Content = source
            };

            foreach (System.Action<BundlerProcess> processor in _bundle.PostProcessors)
            {
                processor(config);
            }

            return config.Content;
        }

        /// <summary>
        /// A list of files used for cache invalidation.
        /// </summary>
        protected override IEnumerable<string> GetFiles(HttpContext context)
        {
            return _bundle.SourceFiles;
        }

        private async Task<string> GetContentAsync(Bundle bundle)
        {
            IEnumerable<string> absolutes = bundle.SourceFiles.Select(f => _fileCache.FileProvider.GetFileInfo(f).PhysicalPath);
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                sb.AppendLine(await File.ReadAllTextAsync(absolute));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        protected override string GetCacheKey(HttpContext context)
        {
            string baseCacheKey = base.GetCacheKey(context);

            string transformKey = string.Join("", _bundle.CacheKeys.Select(p => p.Key + p.Value));
            return (baseCacheKey + transformKey).GetHashCode().ToString();
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    public class BundleMiddleware : BaseMiddleware
    {
        private readonly IHostingEnvironment _env;
        private readonly ITransform _transform;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BundleMiddleware(RequestDelegate next, IHostingEnvironment env, ITransform transform)
            : base(next)
        {
            _env = env;
            _transform = transform;
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected override string ContentType => _transform.ContentType;

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public override async Task<string> ExecuteAsync(HttpContext context)
        {
            string source = await GetContentAsync(_transform);
            return _transform.Transform(context, source);
        }

        private async Task<string> GetContentAsync(ITransform transform)
        {
            IEnumerable<string> absolutes = transform.SourceFiles.Select(f => Path.Combine(_env.WebRootPath, f));
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
            return base.GetCacheKey(context) + _transform.CacheKey;
        }
    }
}

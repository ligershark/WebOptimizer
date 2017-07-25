using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;

namespace Bundler
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    public class BundleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly ITransform _transform;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BundleMiddleware(RequestDelegate next, IHostingEnvironment env, ITransform transform)
        {
            _next = next;
            _env = env;
            _transform = transform;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.ContentType = _transform.ContentType;

            if (context.Request.Query.TryGetValue("v", out var v) && _cache.ContainsKey(v))
            {
                await WriteOutputAsync(context, _cache[v]);
            }
            else
            {
                string source = await GetContentAsync(_transform);
                string transformedBundle = _transform.Transform(context, source);

                await WriteOutputAsync(context, transformedBundle);

                if (!string.IsNullOrEmpty(v))
                {
                    _cache[v] = transformedBundle;
                }
            }
        }

        private async Task WriteOutputAsync(HttpContext context, string content)
        {
            context.Response.ContentType = _transform.ContentType;

            if (context.Request.Query.ContainsKey("v"))
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age=31536000"; // 1 year
                context.Response.Headers["Etag"] = $"\"{context.Request.Query["v"]}\"";
            }

            await context.Response.WriteAsync(content);
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
    }
}

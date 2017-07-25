using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// A base class for response caching middleware.
    /// </summary>
    public abstract class BaseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BaseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected abstract string ContentType { get; }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("v", out var v) && _cache.ContainsKey(v))
            {
                await WriteOutputAsync(context, _cache[v]);
            }
            else
            {
                string result = await ExecuteAsync(context);

                if (string.IsNullOrEmpty(result))
                {
                    await _next(context);
                    return;
                }

                await WriteOutputAsync(context, result);

                if (!string.IsNullOrEmpty(v))
                {
                    _cache[v] = result;
                }
            }
        }

        /// <summary>
        /// Executes the middleware and handles response caching.
        /// </summary>
        public abstract Task<string> ExecuteAsync(HttpContext context);

        private async Task WriteOutputAsync(HttpContext context, string content)
        {
            context.Response.ContentType = ContentType;

            if (context.Request.Query.ContainsKey("v"))
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age=31536000"; // 1 year
                context.Response.Headers["Etag"] = $"\"{context.Request.Query["v"]}\"";
            }

            await context.Response.WriteAsync(content);
        }
    }
}

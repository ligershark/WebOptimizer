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
        private KeyValuePair<string, string> _cache = new KeyValuePair<string, string>();

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
            if (ConditionalGet(context))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, string.Empty);
            }
            else if (context.Request.Query.TryGetValue("v", out var v) && _cache.Key == v)
            {
                await WriteOutputAsync(context, _cache.Value);
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
                    _cache = new KeyValuePair<string, string>(v, result);
                }
            }
        }

        private bool ConditionalGet(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm))
            {
                return _cache.Key == inm.ToString().Trim('"');
            }

            return false;
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

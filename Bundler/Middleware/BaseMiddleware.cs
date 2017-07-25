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
            if (IsConditionalGet(context))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, string.Empty);
                return;
            }

            string cacheKey = GetCacheKey(context);

            if (!string.IsNullOrEmpty(cacheKey) && cacheKey == _cache.Key)
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

                if (!string.IsNullOrEmpty(cacheKey))
                {
                    _cache = new KeyValuePair<string, string>(cacheKey, result);
                }

                await WriteOutputAsync(context, result);
            }
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        protected virtual string GetCacheKey(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("v", out var v))
            {
                return v;
            }

            return null;
        }

        private bool IsConditionalGet(HttpContext context)
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

            if (!string.IsNullOrEmpty(_cache.Key))
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age=31536000"; // 1 year
                context.Response.Headers["Etag"] = $"\"{_cache.Key}\"";
            }

            if (!string.IsNullOrEmpty(content))
            {
                await context.Response.WriteAsync(content);
            }
        }
    }
}

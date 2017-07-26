using System.Collections.Generic;
using System.Threading.Tasks;
using Bundler.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace Bundler
{
    /// <summary>
    /// A base class for response caching middleware.
    /// </summary>
    public abstract class BaseMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly IMemoryCache _cache;
        protected readonly FileCache _fileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BaseMiddleware(RequestDelegate next, IMemoryCache cache, IHostingEnvironment env)
        {
            _next = next;
            //_cache = cache;
            //FileProvider = env.WebRootFileProvider;
            _fileCache = new FileCache(env.WebRootFileProvider, cache);
        }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        //protected IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected abstract string ContentType { get; }

        /// <summary>
        /// A list of files used for cache invalidation.
        /// </summary>
        protected virtual IEnumerable<string> GetFiles(HttpContext context)
        {
            yield return context.Request.Path.Value;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            string cacheKey = GetCacheKey(context);

            if (IsConditionalGet(context, cacheKey))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, string.Empty, cacheKey);
            }
            else if (_fileCache.TryGetValue(cacheKey, out string value))
            {
                await WriteOutputAsync(context, value, cacheKey);
            }
            else
            {
                string result = await ExecuteAsync(context);

                if (string.IsNullOrEmpty(result))
                {
                    await _next(context);
                    return;
                }

                _fileCache.AddFileBundleToCache(cacheKey, result, GetFiles(context));
                //PopulateCache(cacheKey, result, context);

                await WriteOutputAsync(context, result, cacheKey);
            }
        }

        /// <summary>
        /// Executes the middleware and handles response caching.
        /// </summary>
        public abstract Task<string> ExecuteAsync(HttpContext context);

        //private void PopulateCache(string cacheKey, string result, HttpContext context)
        //{
        //    var cacheEntryOptions = new MemoryCacheEntryOptions();

        //    foreach (string file in GetFiles(context))
        //    {
        //        cacheEntryOptions.AddExpirationToken(FileProvider.Watch(file));
        //    }

        //    _cache.Set(cacheKey, result, cacheEntryOptions);
        //}

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        protected virtual string GetCacheKey(HttpContext context)
        {
            string key = context.Request.PathBase + context.Request.Path;

            if (context.Request.Query.TryGetValue("v", out var v))
            {
                key += v;
            }

            return key.GetHashCode().ToString();
        }

        private bool IsConditionalGet(HttpContext context, string cacheKey)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm))
            {
                return cacheKey == inm.ToString().Trim('"');
            }

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, string content, string cacheKey)
        {
            context.Response.ContentType = ContentType;

            if (!string.IsNullOrEmpty(cacheKey))
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age=31536000"; // 1 year
                context.Response.Headers["Etag"] = $"\"{cacheKey}\"";
            }

            if (!string.IsNullOrEmpty(content))
            {
                await context.Response.WriteAsync(content);
            }
        }
    }
}

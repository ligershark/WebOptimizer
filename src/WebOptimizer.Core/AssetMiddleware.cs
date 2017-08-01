using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    internal class AssetMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IAssetPipeline _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetMiddleware"/> class.
        /// </summary>
        public AssetMiddleware(RequestDelegate next, IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline)
        {
            _next = next;
            _env = env;
            _cache = cache;
            _pipeline = pipeline;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public Task InvokeAsync(HttpContext context)
        {
            if (_pipeline.TryFromRoute(context.Request.Path, out IAsset asset))
            {
                return HandleAssetAsync(context, asset);
            }

            return _next(context);
        }

        private async Task HandleAssetAsync(HttpContext context, IAsset asset)
        {
            _pipeline.EnsureDefaults(_env);

            string cacheKey = asset.GenerateCacheKey(context);

            if (IsConditionalGet(context, cacheKey))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, asset, string.Empty, cacheKey).ConfigureAwait(false);
            }
            else if (_pipeline.EnableCaching == true && _cache.TryGetValue(cacheKey, out string value))
            {
                await WriteOutputAsync(context, asset, value, cacheKey).ConfigureAwait(false);
            }
            else
            {
                string result = await asset.ExecuteAsync(context).ConfigureAwait(false);

                if (string.IsNullOrEmpty(result))
                {
                    await _next(context);
                    return;
                }

                AddToCache(cacheKey, result, asset.SourceFiles);

                await WriteOutputAsync(context, asset, result, cacheKey).ConfigureAwait(false);
            }
        }

        private void AddToCache(string cacheKey, string value, IEnumerable<string> files)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            foreach (string file in files)
            {
                cacheOptions.AddExpirationToken(_pipeline.FileProvider.Watch(file));
            }

            _cache.Set(cacheKey, value, cacheOptions);
        }

        private bool IsConditionalGet(HttpContext context, string cacheKey)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm))
            {
                return cacheKey == inm.ToString().Trim('"');
            }

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, IAsset asset, string content, string cacheKey)
        {
            context.Response.ContentType = asset.ContentType;

            if (_pipeline.EnableCaching == true && !string.IsNullOrEmpty(cacheKey))
            {
                context.Response.Headers["Cache-Control"] = $"max-age=31536000"; // 1 year
                context.Response.Headers["ETag"] = $"\"{cacheKey}\"";
            }

            if (!string.IsNullOrEmpty(content))
            {
                await context.Response.WriteAsync(content).ConfigureAwait(false);
            }
        }
    }
}

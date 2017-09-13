using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer
{
    internal class AssetMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IAssetPipeline _pipeline;
        private readonly ILogger _logger;
        internal readonly string _cacheDir;

        public AssetMiddleware(RequestDelegate next, IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, ILogger<AssetMiddleware> logger)
        {
            _next = next;
            _env = env;
            _cache = cache;
            _pipeline = pipeline;
            _logger = logger;

            // Use temp path for unit testing purposes
            string root = string.IsNullOrEmpty(env.ContentRootPath) ? Path.GetTempPath() : env.ContentRootPath;

            _cacheDir = Path.Combine(root, "obj", "WebOptimizerCache");
        }

        public Task InvokeAsync(HttpContext context, IOptionsSnapshot<WebOptimizerOptions> options)
        {
            if (_pipeline.TryGetAssetFromRoute(context.Request.Path, out IAsset asset))
            {
                _logger.LogRequestForAssetStarted(context.Request.Path);
                return HandleAssetAsync(context, asset, options.Value);
            }

            return _next(context);
        }

        private async Task HandleAssetAsync(HttpContext context, IAsset asset, WebOptimizerOptions options)
        {
            options.EnsureDefaults(_env);

            string cacheKey = asset.GenerateCacheKey(context);

            if (_cache.TryGetValue(cacheKey, out MemoryCachedResponse value))
            {
                await WriteOutputAsync(context, asset, value, cacheKey, options).ConfigureAwait(false);
                _logger.LogServedFromMemoryCache(context.Request.Path);
            }
            else if (MemoryCachedResponse.TryGetFromDiskCache(context.Request.Path, cacheKey, _cacheDir, out value))
            {
                await WriteOutputAsync(context, asset, value, cacheKey, options).ConfigureAwait(false);
                AddToCache(cacheKey, value, asset, options);
                _logger.LogServedFromDiskCache(context.Request.Path);
            }
            else
            {
                byte[] bytes = await asset.ExecuteAsync(context, options).ConfigureAwait(false);

                var response = new MemoryCachedResponse(bytes);

                foreach (string name in context.Response.Headers.Keys)
                {
                    response.Headers.Add(name, context.Response.Headers[name]);
                }

                if (bytes == null || bytes.Length == 0)
                {
                    _logger.LogZeroByteResponse(context.Request.Path);
                    await _next(context);
                    return;
                }

                AddToCache(cacheKey, response, asset, options);
                response.CacheToDisk(context.Request.Path, cacheKey, _cacheDir);

                await WriteOutputAsync(context, asset, response, cacheKey, options).ConfigureAwait(false);
                _logger.LogGeneratedOutput(context.Request.Path);
            }
        }

        private void AddToCache(string cacheKey, MemoryCachedResponse value, IAsset asset, WebOptimizerOptions options)
        {
            if (options.EnableCaching == true)
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.SetSlidingExpiration(TimeSpan.FromHours(24));

                foreach (string file in asset.SourceFiles)
                {
                    cacheOptions.AddExpirationToken(asset.GetFileProvider(_env).Watch(file));
                }

                _cache.Set(cacheKey, value, cacheOptions);
            }
        }

        private bool IsConditionalGet(HttpContext context, string cacheKey)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var inm))
            {
                return cacheKey == inm.ToString().Trim('"');
            }

            if (context.Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var ims))
            {
                if (context.Response.Headers.TryGetValue(HeaderNames.LastModified, out var lm))
                {
                    return ims == lm;
                }
            }

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, IAsset asset, MemoryCachedResponse cachedResponse, string cacheKey, WebOptimizerOptions options)
        {
            context.Response.ContentType = asset.ContentType;

            foreach (string name in cachedResponse.Headers.Keys)
            {
                context.Response.Headers[name] = cachedResponse.Headers[name];
            }

            if (!string.IsNullOrEmpty(cacheKey))
            {
                if (options.EnableCaching == true)
                {
                    context.Response.Headers[HeaderNames.CacheControl] = $"max-age=31536000"; // 1 year
                }

                context.Response.Headers[HeaderNames.ETag] = $"\"{cacheKey}\"";

                if (IsConditionalGet(context, cacheKey))
                {
                    context.Response.StatusCode = 304;
                    return;
                }
            }

            if (cachedResponse?.Body?.Length > 0)
            {
                await context.Response.Body.WriteAsync(cachedResponse.Body, 0, cachedResponse.Body.Length);
            }
        }
    }
}

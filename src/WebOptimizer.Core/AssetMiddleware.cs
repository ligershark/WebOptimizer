using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

        public AssetMiddleware(RequestDelegate next, IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, ILogger<AssetMiddleware> logger)
        {
            _next = next;
            _env = env;
            _cache = cache;
            _pipeline = pipeline;
            _logger = logger;
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
                _logger.LogServedFromCache(context.Request.Path);
            }
            else
            {
                byte[] bytes = await asset.ExecuteAsync(context, options).ConfigureAwait(false);

                var response = new MemoryCachedResponse(context.Response.StatusCode, bytes);

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

                AddToCache(cacheKey, response, asset.SourceFiles, options);

                await WriteOutputAsync(context, asset, response, cacheKey, options).ConfigureAwait(false);
                _logger.LogGeneratedOutput(context.Request.Path);
            }
        }

        private void AddToCache(string cacheKey, MemoryCachedResponse value, IEnumerable<string> files, WebOptimizerOptions options)
        {
            if (options.EnableCaching == true)
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.SetSlidingExpiration(TimeSpan.FromHours(24));

                foreach (string file in files)
                {
                    cacheOptions.AddExpirationToken(options.FileProvider.Watch(file));
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

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, IAsset asset, MemoryCachedResponse cachedResponse, string cacheKey, WebOptimizerOptions options)
        {
            context.Response.ContentType = asset.ContentType;

            foreach (string name in cachedResponse.Headers.Keys)
            {
                context.Response.Headers[name] = cachedResponse.Headers[name];
            }

            if (options.EnableCaching == true && !string.IsNullOrEmpty(cacheKey))
            {
                context.Response.Headers[HeaderNames.CacheControl] = $"max-age=31536000"; // 1 year
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

    internal class MemoryCachedResponse
    {
        public MemoryCachedResponse(int statusCode, byte[] body)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public int StatusCode { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public byte[] Body { get; set; }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
    {
        /// <summary>
        /// Adds WebOptimizer to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static void UseWebOptimizer(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var env = (IHostingEnvironment)app.ApplicationServices.GetService(typeof(IHostingEnvironment));

            app.UseMiddleware<AssetMiddleware>();
        }
    }
}

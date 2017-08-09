using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class AssetMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IAssetPipeline _pipeline;
        private readonly Options _options;

        public AssetMiddleware(RequestDelegate next, IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptions<Options> options)
        {
            _next = next;
            _env = env;
            _cache = cache;
            _pipeline = pipeline;
            _options = options.Value;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (_pipeline.TryGetAssetFromRoute(context.Request.Path, out IAsset asset))
            {
                return HandleAssetAsync(context, asset);
            }

            return _next(context);
        }

        private async Task HandleAssetAsync(HttpContext context, IAsset asset)
        {
            _pipeline.EnsureDefaults(_env, _options);

            string cacheKey = asset.GenerateCacheKey(context);

            if (IsConditionalGet(context, cacheKey))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, asset, new byte[0], cacheKey).ConfigureAwait(false);
            }
            else if (_cache.TryGetValue(cacheKey, out byte[] value))
            {
                await WriteOutputAsync(context, asset, value, cacheKey).ConfigureAwait(false);
            }
            else
            {
                byte[] result = await asset.ExecuteAsync(context).ConfigureAwait(false);

                if (result == null || result.Length == 0)
                {
                    await _next(context);
                    return;
                }

                AddToCache(cacheKey, result, asset.SourceFiles);

                await WriteOutputAsync(context, asset, result, cacheKey).ConfigureAwait(false);
            }
        }

        private void AddToCache(string cacheKey, byte[] value, IEnumerable<string> files)
        {
            if (_options.EnableCaching == true)
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.SetSlidingExpiration(TimeSpan.FromHours(24));

                foreach (string file in files)
                {
                    cacheOptions.AddExpirationToken(_pipeline.FileProvider.Watch(file));
                }

                _cache.Set(cacheKey, value, cacheOptions);
            }
        }

        private bool IsConditionalGet(HttpContext context, string cacheKey)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm))
            {
                return cacheKey == inm.ToString().Trim('"');
            }

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, IAsset asset, byte[] content, string cacheKey)
        {
            context.Response.ContentType = asset.ContentType;

            if (_options.EnableCaching == true && !string.IsNullOrEmpty(cacheKey))
            {
                context.Response.Headers["Cache-Control"] = $"max-age=31536000"; // 1 year
                context.Response.Headers["ETag"] = $"\"{cacheKey}\"";
            }

            if (content?.Length > 0)
            {
                await context.Response.Body.WriteAsync(content, 0, content.Length);
            }
        }
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
            var env = (IHostingEnvironment)app.ApplicationServices.GetService(typeof(IHostingEnvironment));

            app.UseMiddleware<AssetMiddleware>();
        }
    }
}

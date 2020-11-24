using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer
{
    internal class AssetMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAssetPipeline _pipeline;
        private readonly ILogger _logger;
        private readonly IAssetBuilder _assetBuilder;

        public AssetMiddleware(RequestDelegate next, IAssetPipeline pipeline, ILogger<AssetMiddleware> logger, IAssetBuilder assetBuilder)
        {
            _next = next;
            _pipeline = pipeline;
            _logger = logger;
            _assetBuilder = assetBuilder;
        }

        public Task InvokeAsync(HttpContext context, IOptionsSnapshot<WebOptimizerOptions> options)
        {
            string path = context.Request.Path.Value;

            if (context.Request.PathBase.HasValue)
            {
                string pathBase = context.Request.PathBase.Value;
                if (path.StartsWith(pathBase))
                {
                    path = path.Substring(pathBase.Length);
                }
            }                

            if (_pipeline.TryGetAssetFromRoute(path, out IAsset asset))
            {
                _logger.LogRequestForAssetStarted(context.Request.Path);
                return HandleAssetAsync(context, asset, options.Value);
            }

            return _next(context);
        }

        private async Task HandleAssetAsync(HttpContext context, IAsset asset, WebOptimizerOptions options)
        {
            IAssetResponse response = await _assetBuilder.BuildAsync(asset, context, options);

            if (response == null)
            {
                await _next(context);
                return;
            }

            await WriteOutputAsync(context, asset, response, response.CacheKey, options);
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

        private async Task WriteOutputAsync(HttpContext context, IAsset asset, IAssetResponse cachedResponse, string cacheKey, WebOptimizerOptions options)
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

                    if (context.Request.Query.ContainsKey("v"))
                    {
                        context.Response.Headers[HeaderNames.CacheControl] += $",immutable";
                    }
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
                SetCompressionMode(context, options);
                await context.Response.Body.WriteAsync(cachedResponse.Body, 0, cachedResponse.Body.Length);
            }
        }

        // Only called when we expect to serve the body.
        private static void SetCompressionMode(HttpContext context, IWebOptimizerOptions options)
        {
            IHttpsCompressionFeature responseCompressionFeature = context.Features.Get<IHttpsCompressionFeature>();
            if (responseCompressionFeature != null)
            {
                responseCompressionFeature.Mode = options.HttpsCompression;
            }
        }
    }
}

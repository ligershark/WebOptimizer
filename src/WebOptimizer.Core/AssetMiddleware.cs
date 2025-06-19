using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer;

internal class AssetMiddleware(RequestDelegate next, IAssetPipeline pipeline, ILogger<AssetMiddleware> logger, IAssetBuilder assetBuilder)
{
    private readonly ILogger _logger = logger;

    public Task InvokeAsync(HttpContext context, IOptionsSnapshot<WebOptimizerOptions> options)
    {
        string? path = context.Request.Path.Value;

        if (context.Request.PathBase.HasValue)
        {
            string pathBase = context.Request.PathBase.Value!;
            if (context.Request.Path.StartsWithSegments(pathBase))
            {
                path = path?[pathBase.Length..];
            }
        }

        if (pipeline.TryGetAssetFromRoute(path!, out var asset))
        {
            _logger.LogRequestForAssetStarted(context.Request.Path);
            return HandleAssetAsync(context, asset, options.Value);
        }

        return next(context);
    }

    private async Task HandleAssetAsync(HttpContext context, IAsset asset, WebOptimizerOptions options)
    {
        var response = await assetBuilder.BuildAsync(asset, context, options);

        if (response is null)
        {
            await next(context);
            return;
        }

        await WriteOutputAsync(context, asset, response, response.CacheKey, options);
    }

    private static bool IsConditionalGet(HttpContext context, string cacheKey)
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

    private static async Task WriteOutputAsync(HttpContext context, IAsset asset, IAssetResponse cachedResponse, string cacheKey, WebOptimizerOptions options)
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
                context.Response.Headers[HeaderNames.CacheControl] = $"{(options.CacheControlAccess != null ? $"{options.CacheControlAccess}," : string.Empty)}max-age=31536000,immutable"; // 1 year, immutable
            }

            context.Response.Headers[HeaderNames.ETag] = $"\"{cacheKey}\"";

            if (IsConditionalGet(context, cacheKey))
            {
                context.Response.StatusCode = 304;
                return;
            }
        }

        if (cachedResponse.Body?.Length > 0)
        {
            SetCompressionMode(context, options);
            await context.Response.Body.WriteAsync(cachedResponse.Body.AsMemory(0, cachedResponse.Body.Length));
        }
    }

    // Only called when we expect to serve the body.
    private static void SetCompressionMode(HttpContext context, WebOptimizerOptions options)
    {
        var responseCompressionFeature = context.Features.Get<IHttpsCompressionFeature>();
        if (responseCompressionFeature is not null)
        {
            responseCompressionFeature.Mode = options.HttpsCompression;
        }
    }
}

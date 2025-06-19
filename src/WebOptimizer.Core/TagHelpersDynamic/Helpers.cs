using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer.Processors;

namespace WebOptimizer.TagHelpersDynamic;

internal static class Helpers
{
    private static readonly ConcurrentDictionary<string, IAsset> _assetCache = new();

    private static readonly Concatenator _concatenator = new();

    private static readonly string[] _emptySourceFiles = [string.Empty];

    internal static IAsset CreateCssAsset(IAssetPipeline pipeline, string key)
    {
        string route = string.Concat("/css/", key, ".css");
        var asset = AddBundleByKey(pipeline, route, "text/css; charset=UTF-8");

        var settings = ServiceExtensions.CssBundlingSettings;

        if (settings.EnforceFileExtensions?.Length > 0)
        {
            asset = (Asset)asset.EnforceFileExtensions(settings.EnforceFileExtensions);
        }

        if (settings.AdjustRelativePaths)
        {
            asset = (Asset)asset.AdjustRelativePaths();
        }

        if (settings.FingerprintUrls)
        {
            asset = (Asset)asset.FingerprintUrls();
        }

        if (settings.Concatenate)
        {
            asset.Processors.Add(_concatenator);
        }

        if (settings.Minify)
        {
            asset = (Asset)asset.MinifyCss(settings.CssSettings);
        }

        return asset;
    }

    internal static IAsset CreateJsAsset(IAssetPipeline pipeline, string key)
    {
        string route = string.Concat("/js/", key, ".js");
        var asset = AddBundleByKey(pipeline, route, "application/javascript; charset=UTF-8");

        var settings = ServiceExtensions.CodeBundlingSettings;

        if (settings.EnforceFileExtensions?.Length > 0)
        {
            asset = (Asset)asset.EnforceFileExtensions(settings.EnforceFileExtensions);
        }

        if (settings.AdjustRelativePaths)
        {
            asset = (Asset)asset.AdjustRelativePaths();
        }

        if (settings.Concatenate)
        {
            asset.Processors.Add(_concatenator);
        }

        if (settings.Minify)
        {
            asset = (Asset)asset.MinifyJavaScript(new JsSettings(settings.CodeSettings));
        }

        return asset;
    }

    internal static bool HandleBundle(Func<IAssetPipeline, string, IAsset> createAsset,
        IAssetPipeline pipeline,
        TagHelperOutput output,
        ActionContext actionContext,
        WebOptimizerOptions options,
        string attrName,
        string attrValue,
        string bundleKey,
        string destBundleKey)
    {
        if (!string.IsNullOrEmpty(bundleKey) && options.EnableTagHelperBundling == true)
        {
            if (string.IsNullOrEmpty(attrValue))
            {
                return true;
            }

            output.SuppressOutput();

            string assetKey = GetKey(actionContext, bundleKey);
            var assetItem = _assetCache.GetOrAdd(assetKey, createAsset(pipeline, assetKey));
            assetItem.TryAddSourceFile(attrValue);

            return true;
        }

        if (!string.IsNullOrEmpty(destBundleKey))
        {
            if (options.EnableTagHelperBundling == false)
            {
                output.SuppressOutput();
                return true;
            }

            string assetKey = GetKey(actionContext, destBundleKey);
            var assetItem = _assetCache.GetOrAdd(assetKey, createAsset(pipeline, assetKey));

            string? pathBase = actionContext.HttpContext?.Request?.PathBase.Value;

            output.Attributes.SetAttribute(attrName, $"{pathBase}{GenerateHash(assetItem, actionContext.HttpContext!, options)}");

            return true;
        }

        return false;
    }

    private static Asset AddBundleByKey(IAssetPipeline pipeline, string route, string contentType)
    {
        var asset = (Asset)pipeline.AddBundle(route, contentType, _emptySourceFiles);
        asset.SourceFiles.Clear();
        return asset;
    }

    /// <summary>
    /// Generates a hash of the files in the bundle. ///
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateHash(IAsset asset, HttpContext httpContext, IWebOptimizerOptions options)
    {
        string hash = asset.GenerateCacheKey(httpContext, options);

        return $"{asset.Route}?v={hash}";
    }

    private static string GetKey(ActionContext actionContext, string key)
    {
        return actionContext.ActionDescriptor switch
        {
            ControllerActionDescriptor controllerAction => string.Concat(controllerAction.ControllerName, controllerAction.ActionName, key),
            CompiledPageActionDescriptor compiledPage => string.Concat(compiledPage.AreaName, compiledPage.ViewEnginePath.Replace("/", ""), key),
            PageActionDescriptor pageAction => string.Concat(pageAction.AreaName, pageAction.ViewEnginePath.Replace("/", ""), key),
            _ => string.Concat(actionContext.ActionDescriptor.DisplayName?.Replace("/", ""), key)
        };
    }
}

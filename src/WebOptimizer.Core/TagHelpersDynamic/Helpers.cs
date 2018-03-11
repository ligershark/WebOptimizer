using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebOptimizer;
    
namespace WebOptimizer.TagHelpersDynamic
{
    internal static class Helpers
    {
        #region AssetCache

        private class AssetItem
        {
            public IAsset Asset { get; set; }
            public bool Initialized { get; set; }
        }

        private static ConcurrentDictionary<string, AssetItem> AssetCache =
            new ConcurrentDictionary<string, AssetItem>();

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static WebOptimizerOptions GetWebOptimizerOptions(this IServiceProvider serviceProvider,
            IHostingEnvironment env)
        {
            var options = ((IOptionsSnapshot<WebOptimizerOptions>)
                    serviceProvider.GetService(typeof(IOptionsSnapshot<WebOptimizerOptions>)))
                .Value;

            EnsureDefaults(options, env);

            return options;
        }

        /// <summary>
        /// Ensures that defaults are set
        /// </summary>
        internal static void EnsureDefaults(IWebOptimizerOptions options, IHostingEnvironment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            options.EnableCaching = options.EnableCaching ?? !env.IsDevelopment();
            options.EnableTagHelperBundling = options.EnableTagHelperBundling ?? true;
        }

        internal static string GetKey(IServiceProvider serviceProvider, string key)
        {
            var actionContextAccessor =
                (IActionContextAccessor) serviceProvider.GetService(typeof(IActionContextAccessor));
            var actionDescriptor = (ControllerActionDescriptor) actionContextAccessor.ActionContext.ActionDescriptor;

            return string.Concat(actionDescriptor.ControllerName, actionDescriptor.ActionName, key);
        }

        private static readonly Concatenator Concatenator = new Concatenator();

        internal static IAsset CreateCssAsset(IAssetPipeline pipeline, string key)
        {
            var route = string.Concat("/css/", key, ".css");
            var asset = AddBundleByKey(pipeline, route, "text/css; charset=UTF-8");

            var settings = ServiceExtensions.CssBundlingSettings;

            if (settings.EnforceFileExtensions?.Length > 0)
            {
                asset = asset.EnforceFileExtensions(settings.EnforceFileExtensions);
            }

            if (settings.AdjustRelativePaths)
            {
                asset = asset.AdjustRelativePaths();
            }

            if (settings.Concatenate)
            {
                asset.Processors.Add(Concatenator);
            }

            if (settings.FingerprintUrls)
            {
                asset = asset.FingerprintUrls();
            }

            if (settings.Minify)
            {
                asset = asset.MinifyCss(settings.CssSettings);
            }

            return asset;
        }


        internal static IAsset CreateJsAsset(IAssetPipeline pipeline, string key)
        {
            var route = string.Concat("/js/", key, ".js");
            var asset = AddBundleByKey(pipeline, route, "application/javascript; charset=UTF-8");

            var settings = ServiceExtensions.CodeBundlingSettings;


            if (settings.EnforceFileExtensions?.Length > 0)
            {
                asset = asset.EnforceFileExtensions(settings.EnforceFileExtensions);
            }

            if (settings.AdjustRelativePaths)
            {
                asset = asset.AdjustRelativePaths();
            }

            if (settings.Concatenate)
            {
                asset.Processors.Add(Concatenator);
            }

            if (settings.Minify)
            {
                asset = asset.MinifyJavaScript(settings.CodeSettings);
            }

            return asset;
        }


        private static readonly string[] EmptySourceFiles = {string.Empty};

        private static IAsset AddBundleByKey(IAssetPipeline pipeline, string route,
            string contentType)
        {
            var asset = (Asset)pipeline.AddBundle(route, contentType, EmptySourceFiles);
            asset.SourceFiles = new HashSet<string>();
            return asset;
        }

        private static AssetItem GetOrCreateAssetByKey(IAssetPipeline pipeline, string key,
            Func<IAssetPipeline, string, IAsset> createAsset)
        {
            AssetItem assetItem;

            if (AssetCache.TryGetValue(key, out assetItem) == false)
            {
                lock (AssetCache)
                {
                    assetItem = AssetCache.GetOrAdd(key, new AssetItem
                    {
                        Asset = createAsset(pipeline, key)
                    });
                }
            }

            return assetItem;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IAssetPipeline GetAssetPipeline(this IServiceProvider serviceProvider)
        {
            var pipeline = (IAssetPipeline) serviceProvider.GetService(typeof(IAssetPipeline));
            return pipeline;
        }

        /// <summary>
        /// Generates a has of the files in the bundle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GenerateHash(IAsset asset, HttpContext httpContext)
        {
            string hash = asset.GenerateCacheKey(httpContext);

            return $"{asset.Route}?v={hash}";
        }


        internal static bool HandleBundle(Func<IAssetPipeline, string, IAsset> createAsset,
            IServiceProvider serviceProvider,
            IHostingEnvironment env,
            TagHelperOutput output,
            HttpContext httpContext,
            string attrName,
            string attrValue,
            string bundleKey,
            string destBundleKey
        )
        {
            if (string.IsNullOrEmpty(bundleKey) == false)
            {
                WebOptimizerOptions options = serviceProvider.GetWebOptimizerOptions(env);

                if (options.EnableTagHelperBundling == true)
                {
                    if (string.IsNullOrEmpty(attrValue))
                    {
                        return true;
                    }

                    output.SuppressOutput();

                    IAssetPipeline pipeline = serviceProvider.GetAssetPipeline();
                    var assetKey = GetKey(serviceProvider, bundleKey);
                    var assetItem = GetOrCreateAssetByKey(pipeline, assetKey, createAsset);
                    if (assetItem.Initialized)
                    {
                        return true;
                    }
                    string cleanRoute = attrValue.TrimStart('~');

                    lock (assetItem.Asset.SourceFiles)
                    {
                        ((HashSet<string>) assetItem.Asset.SourceFiles).Add(cleanRoute);
                    }

                    return true;
                }
            }

            if (string.IsNullOrEmpty(destBundleKey) == false)
            {
                WebOptimizerOptions options = serviceProvider.GetWebOptimizerOptions(env);

                if (options.EnableTagHelperBundling == true)
                {
                    IAssetPipeline pipeline = serviceProvider.GetAssetPipeline();
                    var assetKey = GetKey(serviceProvider, destBundleKey);
                    var assetItem = GetOrCreateAssetByKey(pipeline, assetKey, createAsset);
                    assetItem.Initialized = true;
                    attrValue = GenerateHash(assetItem.Asset, httpContext);

                    output.Attributes.SetAttribute(attrName, attrValue);
                    return true;
                }

                output.SuppressOutput();
                return true;
            }

            return false;
        }
    }
}

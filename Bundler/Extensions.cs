using System;
using System.Collections.Generic;
using Bundler.Processors;
using Bundler.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NUglify.Css;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// Extension methods to register bundles and minifiers.
    /// </summary>
    public static class Extensions
    {
        // TODO: Add this to DI
        /// <summary>
        /// Gets the asset pipeline configuration
        /// </summary>
        public static Pipeline Pipeline { get; } = new Pipeline();

        /// <summary>
        /// Adds Bundler to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        /// <param name="app">The application object.</param>
        /// <param name="assetPipeline">The transform options.</param>
        public static void UseAssetPipeline(this IApplicationBuilder app, Action<Pipeline> assetPipeline)
        {
            assetPipeline(Pipeline);

            AssetMiddleware mw = ActivatorUtilities.CreateInstance<AssetMiddleware>(app.ApplicationServices);

            app.UseRouter(routes =>
            {
                foreach (IAsset asset in Pipeline.Assets)
                {
                    routes.MapGet(asset.Route, context => mw.InvokeAsync(context, asset));
                }
            });
        }

        /// <summary>
        /// Adds a JavaScript asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this Pipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddJs(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a JavaScript asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this Pipeline pipeline, string route, CodeSettings settings, params string[] sourceFiles)
        {
            IAsset asset = pipeline.Add(route, "application/javascript", sourceFiles);
            asset.MinifyJavaScript(settings);

            return asset;
        }

        /// <summary>
        /// Adds a CSS asset to the pipeline.
        /// </summary>
        public static IAsset AddCss(this Pipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddCss(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a CSS asset to the pipeline.
        /// </summary>
        public static IAsset AddCss(this Pipeline pipeline, string route, CssSettings settings, params string[] sourceFiles)
        {
            IAsset asset = pipeline.Add(route, "text/css", sourceFiles);
            asset.MinifyCss(settings);

            return asset;
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IEnumerable<IAsset> Localize<T>(this IEnumerable<IAsset> assets, IApplicationBuilder app)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.Localize<T>(app);
            }
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IAsset Localize<T>(this IAsset asset, IApplicationBuilder app)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(app);
            var localizer = new ScriptLocalizer(stringProvider);

            asset.PostProcessors.Add(localizer);

            return asset;
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset bundle)
        {
            return bundle.MinifyJavaScript(new CodeSettings());
        }

        ///// <summary>
        ///// Runs the JavaScript minifier on the content.
        ///// </summary>
        //public static IEnumerable<IAsset> MinifyJavaScript(this IEnumerable<IAsset> bundle, CodeSettings settings)
        //{
        //    foreach (IAsset asset in bundle)
        //    {
        //        yield return asset.MinifyJavaScript(settings);
        //    }
        //}

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset bundle, CodeSettings settings)
        {
            var minifier = new JavaScriptMinifier(settings);
            bundle.PostProcessors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle)
        {
            return bundle.MinifyCss(new CssSettings());
        }

        ///// <summary>
        ///// Runs the CSS minifier on the content.
        ///// </summary>
        //public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> bundle, CssSettings settings)
        //{
        //    foreach (IAsset asset in bundle)
        //    {
        //        yield return asset.MinifyCss(settings);
        //    }
        //}

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle, CssSettings settings)
        {
            var minifier = new CssMinifier(settings);
            bundle.PostProcessors.Add(minifier);

            return bundle;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Bundler.Processors;
using Bundler.Utilities;
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
        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this Pipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddJs(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this Pipeline pipeline, string route, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "application/javascript", sourceFiles)
                           .Bundle()
                           .MinifyJavaScript(settings);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this Pipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddCss(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this Pipeline pipeline, string route, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .Bundle()
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IEnumerable<IAsset> Localize<T>(this IEnumerable<IAsset> assets)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(AssetManager.Builder);
            var localizer = new ScriptLocalizer(stringProvider);

            foreach (IAsset asset in assets)
            {
                asset.Processors.Add(localizer);
            }

            return assets;
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IAsset Localize<T>(this IAsset asset)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(AssetManager.Builder);
            var localizer = new ScriptLocalizer(stringProvider);

            asset.Processors.Add(localizer);

            return asset;
        }

        /// <summary>
        /// Runs the bundler on the content.
        /// </summary>
        public static IAsset Bundle(this IAsset asset)
        {
            var bundler = new Processors.Concatinator();
            asset.Processors.Add(bundler);

            return asset;
        }

        /// <summary>
        /// Runs the bundler on the content.
        /// </summary>
        public static IEnumerable<IAsset> Bundle(this IEnumerable<IAsset> assets)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.Bundle();
            }
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset asset)
        {
            return asset.MinifyJavaScript(new CodeSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset asset, CodeSettings settings)
        {
            var minifier = new JavaScriptMinifier(settings);
            asset.Processors.Add(minifier);

            return asset;
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJavaScript(this IEnumerable<IAsset> assets)
        {
            return assets.MinifyJavaScript(new CodeSettings()).ToArray();
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJavaScript(this IEnumerable<IAsset> assets, CodeSettings settings)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.MinifyJavaScript(settings);
            }
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle)
        {
            return bundle.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle, CssSettings settings)
        {
            var minifier = new CssMinifier(settings);
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> assets)
        {
            return assets.MinifyCss(new CssSettings()).ToArray();
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> assets, CssSettings settings)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.MinifyCss(settings);
            }
        }
    }
}

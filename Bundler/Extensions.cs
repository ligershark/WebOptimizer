using System;
using System.Globalization;
using Bundler.Processors;
using Bundler.Utilities;
using Microsoft.AspNetCore.Builder;
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
        /// Gets the bundle options.
        /// </summary>
        public static Options Options { get; } = new Options();

        /// <summary>
        /// Adds Bundler to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        /// <param name="app">The application object.</param>
        /// <param name="bundleOptions">The transform options.</param>
        public static void UseBundler(this IApplicationBuilder app, Action<Options> bundleOptions)
        {
            bundleOptions(Options);

            foreach (Bundle bundle in Options.Bundles)
            {
                app.Map(bundle.Route, builder =>
                {
                    builder.UseMiddleware<BundleMiddleware>(bundle);
                });
            }
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static Bundle Localize<T>(this Bundle bundle, IApplicationBuilder app)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(app);

            bundle.PostProcessors.Add(config =>
            {
                CultureInfo culture = LocalizationUtilities.GetRequestUICulture(config);

                config.Bundle.CacheKeys["culture"] = culture.Name;
                config.Content = ScriptLocalizer.Localize(config.Content, stringProvider);
            });

            return bundle;
        }

        ///// <summary>
        ///// Minifies JavaScript files (.js).
        ///// </summary>
        //public static void MinifyJavaScript(this IApplicationBuilder app, CodeSettings settings = null)
        //{
        //    app.UseMiddleware<JavaScriptMiddleware>(settings ?? new CodeSettings());
        //}

        ///// <summary>
        ///// Minifies CSS files (.css).
        ///// </summary>
        //public static void MinifyCss(this IApplicationBuilder app, CssSettings settings = null)
        //{
        //    app.UseMiddleware<CssMiddleware>(settings ?? new CssSettings());
        //}

        ///// <summary>
        ///// Adds a processor to the transformation
        ///// </summary>
        //public static ITransform Run(this ITransform transform, Func<BundlerConfig, string> func)
        //{
        //    transform.PostProcessors.Add(func);
        //    return transform;
        //}

        /// <summary>
        /// Adds a JavaScript bundle.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="route">The route name from where the bundle is served. Example: /my/bundle.js.</param>
        /// <param name="sourceFiles">An array of webroot relative file paths.</param>
        public static Bundle AddJs(this Options options, string route, params string[] sourceFiles)
        {
            return options.AddJs(new CodeSettings(), route, sourceFiles);
        }

        /// <summary>
        /// Adds a JavaScript bundle.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="settings">The JavaScript minification settings.</param>
        /// <param name="route">The route name from where the bundle is served. Example: /my/bundle.js.</param>
        /// <param name="sourceFiles">An array of webroot relative file paths.</param>
        public static Bundle AddJs(this Options options, CodeSettings settings, string route, params string[] sourceFiles)
        {
            var bundle = new Bundle(route, "application/javascript", sourceFiles);
            var minifier = new JavaScriptMinifier(settings);
            bundle.PostProcessors.Add(config => minifier.Execute(config));
            options.Bundles.Add(bundle);

            return bundle;
        }

        /// <summary>
        /// Adds a CSS bundle.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="route">The route name from where the bundle is served. Example: /my/bundle.css.</param>
        /// <param name="sourceFiles">An array of webroot relative file paths.</param>
        public static Bundle AddCss(this Options options, string route, params string[] sourceFiles)
        {
            return options.AddCss(new CssSettings(), route, sourceFiles);
        }

        /// <summary>
        /// Adds a CSS bundle.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="settings">The CSS minification settings.</param>
        /// <param name="route">The route name from where the bundle is served. Example: /my/bundle.css.</param>
        /// <param name="sourceFiles">An array of webroot relative file paths.</param>
        public static Bundle AddCss(this Options options, CssSettings settings, string route, params string[] sourceFiles)
        {
            var bundle = new Bundle(route, "text/css", sourceFiles);
            var minifier = new CssMinifier(settings);
            bundle.PostProcessors.Add(config => minifier.Execute(config));
            options.Bundles.Add(bundle);

            return bundle;
        }
    }
}

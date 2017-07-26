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

            foreach (IBundle bundle in Options.Bundles)
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
        public static IBundle Localize<T>(this IBundle bundle, IApplicationBuilder app)
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

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IBundle MinifyJavaScript(this IBundle bundle)
        {
            return bundle.MinifyJavaScript(new CodeSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IBundle MinifyJavaScript(this IBundle bundle, CodeSettings settings)
        {
            var minifier = new JavaScriptMinifier(settings);
            bundle.PostProcessors.Add(config => minifier.Execute(config));

            return bundle;
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IBundle MinifyCss(this IBundle bundle)
        {
            return bundle.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IBundle MinifyCss(this IBundle bundle, CssSettings settings)
        {
            var minifier = new CssMinifier(settings);
            bundle.PostProcessors.Add(config => minifier.Execute(config));

            return bundle;
        }
    }
}

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WebOptimizer;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        internal static CssBundlingSettings CssBundlingSettings = new CssBundlingSettings();

        internal static CodeBundlingSettings CodeBundlingSettings = new CodeBundlingSettings();

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="env"></param>
        /// <param name="cssBundlingSettings"></param>
        /// <param name="codeBundlingSettings"></param>
        /// <param name="assetPipeline">The web optimization pipeline</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services,
            IWebHostEnvironment env,
            CssBundlingSettings cssBundlingSettings,
            CodeBundlingSettings codeBundlingSettings,
            Action<IAssetPipeline> assetPipeline = null)
        {
            UpdateCssAndCodeBundlingSettings(services, cssBundlingSettings, codeBundlingSettings);

            return services.AddWebOptimizer(pipeline => { assetPipeline?.Invoke(pipeline); });
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="env"></param>
        /// <param name="cssBundlingSettings"></param>
        /// <param name="codeBundlingSettings"></param>
        /// <param name="configureWebOptimizer">WebOptimizer settings</param>
        /// <param name="assetPipeline">The web optimization pipeline</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services,
            IWebHostEnvironment env,
            CssBundlingSettings cssBundlingSettings,
            CodeBundlingSettings codeBundlingSettings,
            Action<WebOptimizerOptions> configureWebOptimizer,
            Action<IAssetPipeline> assetPipeline = null)
        {
            UpdateCssAndCodeBundlingSettings(services, cssBundlingSettings, codeBundlingSettings);

            return services
                .AddWebOptimizer(
                    pipeline => { assetPipeline?.Invoke(pipeline); },
                    configureWebOptimizer);
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="minifyJavaScript">If <code>true</code>; calls <code>AddJs()</code> on the pipeline.</param>
        /// <param name="minifyCss">If <code>true</code>; calls <code>AddCss()</code> on the pipeline.</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services, bool minifyJavaScript = true, bool minifyCss = true)
        {
            return services.AddWebOptimizer(pipeline => ConfigurePipeline(pipeline, minifyJavaScript, minifyCss));
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="minifyJavaScript">If <code>true</code>; calls <code>AddJs()</code> on the pipeline.</param>
        /// <param name="minifyCss">If <code>true</code>; calls <code>AddCss()</code> on the pipeline.</param>
        /// <param name="configureWebOptimizer">WebOptimizer settings</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services, Action<WebOptimizerOptions> configureWebOptimizer, bool minifyJavaScript = true, bool minifyCss = true)
        {
            return services
                .AddWebOptimizer(
                    pipeline => ConfigurePipeline(pipeline, minifyJavaScript, minifyCss),
                    configureWebOptimizer);
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assetPipeline">The web optimization pipeline</param>
        /// <param name="configureWebOptimizer">WebOptimizer settings</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services, Action<IAssetPipeline> assetPipeline, Action<WebOptimizerOptions> configureWebOptimizer)
        {
            return services.RegisterComponents(assetPipeline,
                ServiceDescriptor.Singleton<IConfigureOptions<WebOptimizerOptions>>(_ =>
                    new InCodeWebOptimizerConfig(configureWebOptimizer)));
        }


        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services,
            Action<IAssetPipeline> assetPipeline)
        {
            return services.RegisterComponents(assetPipeline,
                ServiceDescriptor.Transient<IConfigureOptions<WebOptimizerOptions>, WebOptimizerConfig>());
        }

        private static IServiceCollection RegisterComponents(this IServiceCollection services,
            Action<IAssetPipeline> assetPipeline, ServiceDescriptor configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (assetPipeline == null)
            {
                throw new ArgumentNullException(nameof(assetPipeline));
            }

            var pipeline = new AssetPipeline();
            assetPipeline(pipeline);

            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<IAssetResponseStore, AssetResponseStore>();
            services.TryAddSingleton<IAssetPipeline>(factory => pipeline);
            services.TryAddSingleton<IAssetBuilder, AssetBuilder>();
            services.TryAddEnumerable(configureOptions);

            return services;
        }

        private static void ConfigurePipeline(IAssetPipeline pipeline, bool minifyJavaScript, bool minifyCss)
        {
            if (minifyCss)
            {
                pipeline.MinifyCssFiles();
            }

            if (minifyJavaScript)
            {
                pipeline.MinifyJsFiles();
            }
        }

        private static void UpdateCssAndCodeBundlingSettings(IServiceCollection services,
            CssBundlingSettings cssBundlingSettings, CodeBundlingSettings codeBundlingSettings)
        {
            if (cssBundlingSettings == null) throw new ArgumentNullException(nameof(cssBundlingSettings));
            if (codeBundlingSettings == null) throw new ArgumentNullException(nameof(codeBundlingSettings));

            CssBundlingSettings = cssBundlingSettings;
            CodeBundlingSettings = codeBundlingSettings;

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }
    }
}
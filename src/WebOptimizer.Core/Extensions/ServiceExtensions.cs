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
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services,
            IWebHostEnvironment env,
            CssBundlingSettings cssBundlingSettings,
            CodeBundlingSettings codeBundlingSettings, Action<IAssetPipeline> assetPipeline = null)
        {
            if (cssBundlingSettings == null) throw new ArgumentNullException(nameof(cssBundlingSettings));
            if (codeBundlingSettings == null) throw new ArgumentNullException(nameof(codeBundlingSettings));

            CssBundlingSettings = cssBundlingSettings;
            CodeBundlingSettings = codeBundlingSettings;

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddWebOptimizer(pipeline =>
            {
                assetPipeline?.Invoke(pipeline);
            });

            return services;
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="minifyJavaScript">If <code>true</code>; calls <code>AddJs()</code> on the pipeline.</param>
        /// <param name="minifyCss">If <code>true</code>; calls <code>AddCss()</code> on the pipeline.</param>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services, bool minifyJavaScript = true, bool minifyCss = true)
        {
            services.AddWebOptimizer(pipeline =>
            {
                if (minifyCss)
                {
                    pipeline.MinifyCssFiles();
                }

                if (minifyJavaScript)
                {
                    pipeline.MinifyJsFiles();
                }
            });

            return services;
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddWebOptimizer(this IServiceCollection services, Action<IAssetPipeline> assetPipeline)
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
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<WebOptimizerOptions>, WebOptimizerConfig>());

            return services;
        }
    }
}

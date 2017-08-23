using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class ServiceExtensions
    {
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
            services.AddSingleton<IAssetPipeline, AssetPipeline>(factory => pipeline);
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<WebOptimizerOptions>, WebOptimizerConfig>());

            return services;
        }
    }
}

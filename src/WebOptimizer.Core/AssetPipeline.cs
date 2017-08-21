using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class AssetPipeline : IAssetPipeline
    {
        private List<IAsset> _assets = new List<IAsset>();

        public IReadOnlyList<IAsset> Assets => _assets;

        public bool TryGetAssetFromRoute(string route, out IAsset asset)
        {
            asset = null;

            // Bail if this is an absolute path
            if (route.StartsWith("//") || route.Contains("://"))
            {
                return false;
            }

            string cleanRoute = NormalizeRoute(route);

            // First check direct matches
            foreach (IAsset existing in Assets)
            {
                if (existing.Route.Equals(cleanRoute, StringComparison.OrdinalIgnoreCase))
                {
                    asset = existing;
                    return true;
                }
            }

            // Then check globbing matches
            if (route != "/")
            {
                foreach (IAsset existing in Assets)
                {
                    var matcher = new Matcher();
                    matcher.AddInclude(existing.Route);

                    if (matcher.Match(cleanRoute.TrimStart('/')).HasMatches)
                    {
                        asset = new Asset(cleanRoute, existing.ContentType, new[] { cleanRoute });

                        foreach (IProcessor processor in existing.Processors)
                        {
                            asset.Processors.Add(processor);
                        }

                        _assets.Add(asset);
                        return true;
                    }
                }
            }

            return false;
        }

        public IAsset AddBundle(IAsset asset)
        {
            return AddBundle(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
        }

        public IEnumerable<IAsset> AddBundle(IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                IAsset ass = AddBundle(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
                list.Add(ass);
            }

            return list;
        }

        public IAsset AddBundle(string route, string contentType, params string[] sourceFiles)
        {
            if (sourceFiles.Length == 0)
            {
                throw new ArgumentException("At least one source file has to be specified", nameof(sourceFiles));
            }

            if (route.Contains("*") || route.Contains("[") && route.Contains("?"))
            {
                throw new ArgumentException($"The route \"{route}\" appears to be a globbing pattern which isn't supported for bundle routes.", nameof(route));
            }

            route = NormalizeRoute(route);

            if (Assets.Any(a => a.Route.Equals(route, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"The route \"{route}\" was already specified", nameof(route));
            }

            IAsset asset = new Asset(route, contentType, sourceFiles);
            _assets.Add(asset);

            return asset;
        }

        public IEnumerable<IAsset> AddFiles(string contentType, params string[] sourceFiles)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("A valid content type must be specified", nameof(contentType));
            }

            if (sourceFiles.Length == 0)
            {
                throw new ArgumentException("At least one source file has to be specified", nameof(sourceFiles));
            }

            var list = new List<IAsset>();

            foreach (string file in sourceFiles)
            {
                IAsset asset = new Asset(NormalizeRoute(file), contentType, new[] { file });
                list.Add(asset);
            }

            _assets.AddRange(list);

            return list;
        }

        private string NormalizeRoute(string route)
        {
            return "/" + route.Trim().TrimStart('~', '/');
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
    {
        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/> and enables CSS and JavaScript minification.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="minifyJavaScript">If <code>true</code>; calls <code>AddJs()</code> on the pipeline.</param>
        /// <param name="minifyCss">If <code>true</code>; calls <code>AddCss()</code> on the pipeline.</param>
        public static IAssetPipeline AddWebOptimizer(this IServiceCollection services, bool minifyJavaScript = true, bool minifyCss = true)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var pipeline = new AssetPipeline();

            if (minifyCss)
            {
                pipeline.MinifyCssFiles();
            }

            if (minifyJavaScript)
            {
                pipeline.MinifyJsFiles();
            }

            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.AddSingleton<IAssetPipeline, AssetPipeline>(factory => pipeline);

            return pipeline;
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IAssetPipeline AddWebOptimizer(this IServiceCollection services, Action<IAssetPipeline> assetPipeline)
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

            return pipeline;
        }
    }
}

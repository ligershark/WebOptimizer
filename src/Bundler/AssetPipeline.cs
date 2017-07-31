using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUglify.Css;
using NUglify.JavaScript;

namespace Bundler
{
    internal class AssetPipeline : IAssetPipeline
    {
        private List<IAsset> _assets = new List<IAsset>();

        public bool? EnabledBundling { get; set; }

        public bool? EnableCaching { get; set; }

        /// <summary>
        /// Gets a list of transforms added.
        /// </summary>
        public IReadOnlyList<IAsset> Assets => _assets;

        public IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Gets the <see cref="IAsset" /> from the specified route.
        /// </summary>
        /// <param name="route">The route to find the asset by.</param>
        public IAsset FromRoute(string route)
        {
            route = route.TrimStart('/');

            foreach (IAsset asset in Assets)
            {
                if (asset.Route.Equals(route, StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            return null;
        }

        public IAsset Add(IAsset asset)
        {
            return Add(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
        }

        public IEnumerable<IAsset> Add(IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                IAsset ass = Add(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
                list.Add(ass);
            }

            return list;
        }

        public IAsset Add(string route, string contentType, params string[] sourceFiles)
        {
            route = route.TrimStart('/');

            if (FromRoute(route) != null)
            {
                throw new ArgumentException($"The route \"{route}\" was already specified", nameof(route));
            }

            string[] sources = sourceFiles;

            if (sourceFiles.Length == 0)
            {
                sources = new[] { route };
            }

            IAsset asset = Asset.Create(route, contentType, sources);
            _assets.Add(asset);

            return asset;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Adds WebOptimizer to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static void UseWebOptimizer(this IApplicationBuilder app)
        {
            app.UseMiddleware<AssetMiddleware>();
        }

        /// <summary>
        /// Adds WebOptimizer to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static void AddWebOptimizer(this IServiceCollection services, Action<IAssetPipeline> assetPipeline)
        {
            var pipeline = new AssetPipeline();
            assetPipeline(pipeline);

            services.AddSingleton<IAssetPipeline, AssetPipeline>(factory => pipeline);
        }

        /// <summary>
        /// Ensures that defaults are set
        /// </summary>
        public static void EnsureDefaults(this IAssetPipeline pipeline, IHostingEnvironment env)
        {
            pipeline.FileProvider = pipeline.FileProvider ?? env.WebRootFileProvider;
            pipeline.EnableCaching = pipeline.EnableCaching ?? !env.IsDevelopment();
            pipeline.EnabledBundling = pipeline.EnableCaching ?? true;
        }

        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddJs(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this IAssetPipeline pipeline, string route, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "application/javascript", sourceFiles)
                           .Bundle()
                           .MinifyJavaScript(settings);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddCss(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this IAssetPipeline pipeline, string route, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .Bundle()
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Adds loose files to the optimization pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css".</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to optimize.</param>
        /// <returns></returns>
        public static IEnumerable<IAsset> AddFiles(this IAssetPipeline pipeline, string contentType, params string[] sourceFiles)
        {
            var list = new List<IAsset>();

            foreach (string file in sourceFiles)
            {
                IAsset asset = pipeline.Add(file, contentType, file);
                asset.Processors.Add(new Concatinator());
                list.Add(asset);
            }

            return list;
        }
    }
}

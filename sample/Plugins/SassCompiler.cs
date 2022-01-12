using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SharpScss;
using WebOptimizer;
using WebOptimizerDemo;

namespace WebOptimizerDemo
{
    /// <summary>
    /// Compiles Sass files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class SassCompiler : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context, IAssetContext config) => string.Empty;

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)context.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = context.Asset.GetFileProvider(env);

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = fileProvider.GetFileInfo(route);
                var settings = new ScssOptions { InputFile = file.PhysicalPath };

                ScssResult result = Scss.ConvertToCss(context.Content[route].AsString(), settings);

                content[route] = result.Css.AsByteArray();
            }

            context.Content = content;

            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for registrating the Sass compiler on the Asset Pipeline.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.Processors.Add(new SassCompiler());
            return asset;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.CompileScss());
            }

            return list;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        /// <param name="route">The route where the compiled .css file will be available from.</param>
        /// <param name="sourceFiles">The path to the .sass or .scss source files to compile.</param>
        public static IAsset AddScssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                           .CompileScss()
                           .AdjustRelativePaths()
                           .Concatenate()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        /// <summary>
        /// Compiles .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", "**/*.scss")
                           .CompileScss()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        /// <summary>
        /// Compiles the specified .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/css; charset=UFT-8", sourceFiles)
                           .CompileScss()
                           .FingerprintUrls()
                           .MinifyCss();
        }
    }
}

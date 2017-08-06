using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.JavaScript;

namespace WebOptimizer
{
    internal class JavaScriptMinifier : IProcessor
    {
        public JavaScriptMinifier(CodeSettings settings)
        {
            Settings = settings;
        }

        public string CacheKey(HttpContext context) => string.Empty;

        public CodeSettings Settings { get; set; }

        public Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min.js"))
                    continue;
                string input = config.Content[key].AsString();
                UglifyResult result = Uglify.Js(input, Settings);
                string minified = result.Code;

                if (result.HasErrors)
                {
                    minified = $"/* {string.Join("\r\n", result.Errors)} */\r\n" + input;
                }

                content[key] = minified.AsByteArray();
            }

            config.Content = content;

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
    {
        /// <summary>
        /// Dynamically adds all requested .js files to the pipeline.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline)
        {
            return pipeline.MinifyJsFiles(new CodeSettings());
        }

        /// <summary>
        /// Dynamically adds all requested .js files to the pipeline.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, CodeSettings settings)
        {
            return pipeline.AddFiles("application/javascript; charset=UTF-8", "**/*.js")
                           .MinifyJavaScript(settings);
        }

        /// <summary>
        /// Minifies the specified .js files
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyJsFiles(new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies the specified .js files
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddFiles("application/javascript; charset=UTF-8", sourceFiles)
                           .MinifyJavaScript(settings);
        }

        /// <summary>
        /// Creates a JavaScript bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddJavaScriptBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddJavaScriptBundle(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a JavaScript bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddJavaScriptBundle(this IAssetPipeline pipeline, string route, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "application/javascript; charset=UTF-8", sourceFiles)
                           .Concatenate()
                           .MinifyJavaScript(settings);
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
            return assets.MinifyJavaScript(new CodeSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJavaScript(this IEnumerable<IAsset> assets, CodeSettings settings)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.MinifyJavaScript(settings));
            }

            return list;
        }
    }
}

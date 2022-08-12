using System.Collections.Generic;
using System.Threading.Tasks;
using NUglify;
using NUglify.JavaScript;
using WebOptimizer;

namespace WebOptimizer
{
    internal class JavaScriptMinifier : Processor
    {
        public JavaScriptMinifier(CodeSettings settings)
        {
            Settings = settings;
        }

        public CodeSettings Settings { get; set; }

        public override Task ExecuteAsync(IAssetContext config)
        {
            if (!Settings.MinifyCode) return Task.CompletedTask;
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min") || key.EndsWith(".min.js"))
                {
                    content[key] = config.Content[key];
                    continue;
                }

                string input = config.Content[key].AsString();
                string minified;
                try
                {
                    UglifyResult result = Uglify.Js(input, Settings);
                    minified = result.Code;
                    if (result.HasErrors)
                    {
                        minified = $"/* {string.Join("\r\n", result.Errors)} */\r\n" + input;
                    }
                }
                catch
                {
                    //If there's an error minifying, then use the original uminified value
                    minified = input + "/* Exception caught attempting to minify */";
                }

                content[key] = minified.AsByteArray();
            }

            config.Content = content;

            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Minifies any .js file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline)
        {
            return pipeline.MinifyJsFiles(new CodeSettings());
        }

        /// <summary>
        /// Minifies any .js file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, CodeSettings settings)
        {
            return pipeline.MinifyJsFiles(settings, "**/*.js");
        }

        /// <summary>
        /// Minifies the specified .js files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyJsFiles(new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies tje specified .js files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/javascript; charset=UTF-8", sourceFiles)
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
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
            return pipeline.AddBundle(route, "text/javascript; charset=UTF-8", sourceFiles)
                           .EnforceFileExtensions(".js", ".jsx", ".es5", ".es6")
                           .Concatenate()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
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
            return assets.AddProcessor(asset => asset.MinifyJavaScript(settings));
        }
    }
}

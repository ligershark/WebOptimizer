using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUglify;
using NUglify.JavaScript;
using WebOptimizer;
using WebOptimizer.Processors;

namespace WebOptimizer
{
    internal class JavaScriptMinifier : Processor
    {
        public JavaScriptMinifier(JsSettings settings)
        {
            Settings = settings;
        }

        public JsSettings Settings { get; set; }

        public override Task ExecuteAsync(IAssetContext config)
        {
            if (!Settings.CodeSettings.MinifyCode) return Task.CompletedTask;
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
                    UglifyResult result;
                    string sourceMapContent = null;

                    // If .AddJavascriptBundle setup the SourceMap Asset, it will be assigned here, we need to fill it
                    var sourceMapAsset = Settings.PipelineSourceMap;
                    if (sourceMapAsset != null)
                    {
                        // Setup the side-effects writing of the SourceMap file
                        var sb = new StringBuilder();
                        using (var sw = new StringWriter(sb))
                        {
                            using (var sourceMap = new V3SourceMap(sw))
                            {
                                // Causes the side-effect writing of the SourceMap to our StringWriter...
                                Settings.CodeSettings.SymbolsMap = sourceMap;
                                sourceMap.MakePathsRelative = false;
                                sourceMap.StartPackage(config.Asset.Route, sourceMapAsset.Route);

                                result = Uglify.Js(input, Settings.CodeSettings);
                            }
                            // These Dispose steps cause the actual flush of the content to the StringBuilder
                        }
                        sourceMapContent = sb.ToString();
                    }
                    else
                    {
                        result = Uglify.Js(input, Settings.CodeSettings);
                    }

                    minified = result.Code;

                    if (result.HasErrors)
                    {
                        minified = $"/* {string.Join("\r\n", result.Errors)} */\r\n" + input;
                    }
                    else
                    {
                        if (sourceMapContent != null)
                        {
                            // Successful minification, and source map generation succeeded, write out to its separate Asset/Route
                            sourceMapAsset.Items["Content"] = sourceMapContent;
                        }
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
            return pipeline.MinifyJsFiles(new JsSettings(settings), "**/*.js");
        }

        /// <summary>
        /// Minifies the specified .js files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyJsFiles(new JsSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies the specified .js files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJsFiles(this IAssetPipeline pipeline, JsSettings settings, params string[] sourceFiles)
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
            return pipeline.AddJavaScriptBundle(route, new JsSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a JavaScript bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddJavaScriptBundle(this IAssetPipeline pipeline, string route, JsSettings settings, params string[] sourceFiles)
        {
            var bundleAsset = pipeline.AddBundle(route, "text/javascript; charset=UTF-8", sourceFiles)
                .EnforceFileExtensions(".js", ".jsx", ".es5", ".es6")
                .Concatenate()
                .AddResponseHeader("X-Content-Type-Options", "nosniff")
                .MinifyJavaScript(settings);

            if (settings.GenerateSourceMap)
            {
                // A simple config flag saying to generate a SourceMap - the legwork is on the framework
                // Nuglify returns minified Javascript while generating a SourceMap as a side effect, like it or not, and
                // It's not possible to ask for a map until the first time the minified code is delivered (since the map is in the comments of the min bundle),
                // so we add a null route/Asset to the pipeline for now, and we'll fill it in later on first request of the bundle
                string mapRoute = route.Replace(".js", ".map.js");
                var sourceMapAsset = pipeline.AddAsset(mapRoute, "application/json")
                    .UseItemContent();
                settings.PipelineSourceMap = sourceMapAsset;
            }

            return bundleAsset;
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset asset)
        {
            return asset.MinifyJavaScript(new JsSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IAsset MinifyJavaScript(this IAsset asset, JsSettings settings)
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
            return assets.MinifyJavaScript(new JsSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyJavaScript(this IEnumerable<IAsset> assets, JsSettings settings)
        {
            return assets.AddProcessor(asset => asset.MinifyJavaScript(settings));
        }
    }
}

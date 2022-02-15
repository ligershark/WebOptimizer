using System.Collections.Generic;
using System.Threading.Tasks;
using NUglify;
using NUglify.Css;
using WebOptimizer;

namespace WebOptimizer
{
    internal class CssMinifier : Processor
    {
        public CssMinifier(CssSettings settings)
        {
            Settings = settings;
        }

        public CssSettings Settings { get; set; }

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min") || key.EndsWith(".min.css"))
                {
                    content[key] = config.Content[key];
                    continue;
                }

                string input = config.Content[key].AsString();
                UglifyResult result = Uglify.Css(input, Settings);
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
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Minifies any .css file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline)
        {
            return pipeline.MinifyCssFiles(new CssSettings());
        }

        /// <summary>
        /// Minifies any .css file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline, CssSettings settings)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", "**/*.css")
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss(settings);
        }


        /// <summary>
        /// Minifies the specified .css files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyCssFiles(new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies the specified .css files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", sourceFiles)
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Creates a CSS bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddCssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddCssBundle(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a CSS bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddCssBundle(this IAssetPipeline pipeline, string route, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                           .EnforceFileExtensions(".css")
                           .AdjustRelativePaths()
                           .Concatenate()
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset asset)
        {
            return asset.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset asset, CssSettings settings)
        {
            var minifier = new CssMinifier(settings);
            asset.Processors.Add(minifier);

            return asset;
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> assets)
        {
            return assets.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> assets, CssSettings settings)
        {
            return assets.AddProcessor(asset => asset.MinifyCss(settings));
        }
    }
}
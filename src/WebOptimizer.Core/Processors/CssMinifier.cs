using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.Css;

namespace WebOptimizer
{
    internal class CssMinifier : IProcessor
    {
        public CssMinifier(CssSettings settings)
        {
            Settings = settings;
        }

        public string CacheKey(HttpContext context) => string.Empty;

        public CssSettings Settings { get; set; }

        public Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min.css"))
                    continue;

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

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
    {
        /// <summary>
        /// Minifies and fingerprints any .css file requested.
        /// </summary>
        public static IAsset MinifyCssFiles(this IAssetPipeline pipeline) =>
            pipeline.MinifyCssFiles(new CssSettings());

        /// <summary>
        /// Minifies and fingerprints any .css file requested.
        /// </summary>
        public static IAsset MinifyCssFiles(this IAssetPipeline pipeline, CssSettings settings)
        {
            return pipeline.AddFileExtension(".css", "text/css; charset=UTF-8")
                           .FingerprintUrls()
                           .MinifyCss(settings);
        }


        /// <summary>
        /// Minifies the specified .css files
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyCssFiles(new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies the specified .css files
        /// </summary>
        public static IEnumerable<IAsset> MinifyCssFiles(this IAssetPipeline pipeline, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", sourceFiles)
                           .FingerprintUrls()
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
                           .AdjustRelativePaths()
                           .Concatinate()
                           .FingerprintUrls()
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle)
        {
            return bundle.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the CSS minifier on the content.
        /// </summary>
        public static IAsset MinifyCss(this IAsset bundle, CssSettings settings)
        {
            var minifier = new CssMinifier(settings);
            bundle.Processors.Add(minifier);

            return bundle;
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
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.MinifyCss(settings));
            }

            return list;
        }
    }
}

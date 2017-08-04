using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.Css;

namespace WebOptimizer
{
    /// <summary>
    /// A processor that minifies JavaScript
    /// </summary>
    internal class CssMinifier : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssMinifier"/> class.
        /// </summary>
        public CssMinifier(CssSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context) => string.Empty;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CssSettings Settings { get; set; }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min.css"))
                    continue;

                UglifyResult result = Uglify.Css(config.Content[key].AsString(), Settings);
                string minified = result.Code;

                if (result.HasErrors)
                {
                    minified = $"/* {string.Join("\r\n", result.Errors)} */";
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
        /// Runs the JavaScript minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyCss(this IEnumerable<IAsset> assets)
        {
            return assets.MinifyCss(new CssSettings());
        }

        /// <summary>
        /// Runs the JavaScript minifier on the content.
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

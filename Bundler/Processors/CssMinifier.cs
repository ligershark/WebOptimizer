using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.Css;

namespace Bundler.Processors
{
    /// <summary>
    /// A processor that minifies JavaScript
    /// </summary>
    public class CssMinifier : IProcessor
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
        public void Execute(IAssetContext config)
        {
            UglifyResult minified = Uglify.Css(config.Content, Settings);

            if (!minified.HasErrors)
            {
                config.Content = minified.Code;
            }
        }
    }
}

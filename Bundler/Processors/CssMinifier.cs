using NUglify;
using NUglify.Css;

namespace Bundler.Processors
{
    /// <summary>
    /// A processor that minifies JavaScript
    /// </summary>
    public class CssMinifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssMinifier"/> class.
        /// </summary>
        public CssMinifier(CssSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CssSettings Settings { get; set; }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public void Execute(BundlerContext config)
        {
            UglifyResult minified = Uglify.Css(config.Content, Settings);

            if (!minified.HasErrors)
            {
                config.Content = minified.Code;
            }
        }
    }
}

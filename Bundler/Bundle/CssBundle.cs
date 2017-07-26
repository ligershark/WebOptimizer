using NUglify.Css;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class CssBundle : Bundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssBundle"/> class.
        /// </summary>
        public CssBundle(string route, params string[] sourceFiles)
            : this(route, new CssSettings(), sourceFiles)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsBundle"/> class.
        /// </summary>
        public CssBundle(string route, CssSettings settings, params string[] sourceFiles)
            : base(route, "text/css", sourceFiles)
        {
            this.MinifyCss(settings);
        }
    }
}

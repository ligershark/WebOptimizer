using Bundler.Processors;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class JsBundle : Bundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsBundle"/> class.
        /// </summary>
        public JsBundle(string route, params string[] sourceFiles)
            : this(route, new CodeSettings(), sourceFiles)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsBundle"/> class.
        /// </summary>
        public JsBundle(string route, CodeSettings settings, params string[] sourceFiles)
            : base(route, "application/javascript", sourceFiles)
        {
            this.MinifyJavaScript(settings);
        }
    }
}

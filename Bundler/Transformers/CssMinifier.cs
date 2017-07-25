using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.Css;

namespace Bundler.Transformers
{
    /// <summary>
    /// A CSS minifier.
    /// </summary>
    public class CssMinifier : BaseTransform
    {
        private CssSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CssMinifier"/> class.
        /// </summary>
        public CssMinifier(string path)
            : base(path)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CssMinifier"/> class.
        /// </summary>
        public CssMinifier(string path, CssSettings settings)
            : base(path)
        {
            _settings = settings;
        }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public override string ContentType => "text/css";

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        protected override string Run(HttpContext context, string source)
        {
            CssSettings settings = _settings ?? new CssSettings();
            UglifyResult minified = Uglify.Css(source, settings);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

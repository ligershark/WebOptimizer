using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.JavaScript;

namespace Bundler.Transformers
{
    /// <summary>
    /// A JavaScript minifier
    /// </summary>
    public class JavaScriptMinifier : BaseTransform
    {
        private CodeSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMinifier"/> class.
        /// </summary>
        public JavaScriptMinifier(string path)
            : base(path)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMinifier"/> class.
        /// </summary>
        public JavaScriptMinifier(string path, CodeSettings settings)
            : base(path)
        {
            _settings = settings;
        }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public override string ContentType => "application/javascript";

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        public override string Transform(HttpContext context, string source)
        {
            CodeSettings settings = _settings ?? new CodeSettings();
            UglifyResult minified = Uglify.Js(source, settings);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

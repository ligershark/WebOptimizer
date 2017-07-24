using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.Css;

namespace Bundler.Transformers
{
    public class CssMinifier : BaseTransform
    {
        public CssMinifier(string path) : base(path)
        { }

        public CssMinifier(string path, CssSettings settings) : base(path)
        {
            Settings = settings;
        }

        public override string ContentType => "text/css";

        public CssSettings Settings { get; }

        public override string Transform(HttpContext context, string source)
        {
            var settings = Settings ?? new CssSettings();
            var minified = Uglify.Css(source, settings);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

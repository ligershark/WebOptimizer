using Microsoft.AspNetCore.Http;
using NUglify;

namespace Bundler.Transformers
{
    public class CssMinifier : BaseTransform
    {
        public CssMinifier(string path) : base(path)
        { }

        public override string ContentType => "text/css";

        public override string Transform(HttpContext context, string source)
        {
            var minified = Uglify.Css(source);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

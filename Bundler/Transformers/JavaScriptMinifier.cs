using Microsoft.AspNetCore.Http;
using NUglify;

namespace Bundler.Transformers
{
    public class JavaScriptMinifier : BaseTransform
    {
        public JavaScriptMinifier(string path) : base(path)
        { }

        public override string ContentType => "application/javascript";

        public override string Transform(HttpContext context, string source)
        {
            var minified = Uglify.Js(source);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

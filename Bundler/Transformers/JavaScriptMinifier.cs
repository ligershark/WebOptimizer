using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.JavaScript;

namespace Bundler.Transformers
{
    public class JavaScriptMinifier : BaseTransform
    {
        public JavaScriptMinifier(string path) : base(path)
        { }

        public JavaScriptMinifier(string path, CodeSettings settings) : base(path)
        {
            Settings = settings;
        }

        public CodeSettings Settings { get; }

        public override string ContentType => "application/javascript";

        public override string Transform(HttpContext context, string source)
        {
            var settings = Settings ?? new CodeSettings();
            var minified = Uglify.Js(source, settings);

            if (minified.HasErrors)
                return null;

            return minified.Code;
        }
    }
}

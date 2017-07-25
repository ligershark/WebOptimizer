using Bundler.Transformers;
using Microsoft.AspNetCore.Http;

namespace Bundler
{
    public class BundlerConfig
    {
        public BundlerConfig(HttpContext httpContext, ITransform transform)
        {
            HttpContext = httpContext;
            Transform = transform;
        }

        public string Content { get; set; }
        public HttpContext HttpContext { get; }
        public ITransform Transform { get; }
    }
}

using Bundler.Transformers;
using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// A configuration object for Bundler.
    /// </summary>
    public class BundlerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BundlerConfig"/> class.
        /// </summary>
        public BundlerConfig(HttpContext httpContext, ITransform transform)
        {
            HttpContext = httpContext;
            Transform = transform;
        }

        /// <summary>
        /// Gets or sets the content of the response.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the transform.
        /// </summary>
        public ITransform Transform { get; }
    }
}

using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// A configuration object for Bundler.
    /// </summary>
    public class BundlerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BundlerContext"/> class.
        /// </summary>
        public BundlerContext(HttpContext httpContext, IBundle bundle)
        {
            HttpContext = httpContext;
            Bundle = bundle;
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
        public IBundle Bundle { get; }
    }
}

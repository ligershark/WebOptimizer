using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// A configuration object for Bundler.
    /// </summary>
    public class AssetContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetContext"/> class.
        /// </summary>
        public AssetContext(HttpContext httpContext, IAsset asset)
        {
            HttpContext = httpContext;
            Asset = asset;
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
        public IAsset Asset { get; }
    }
}

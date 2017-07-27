using Microsoft.AspNetCore.Http;

namespace Bundler
{
    /// <summary>
    /// The context used to perform processing to <see cref="IAsset"/> instances.
    /// </summary>
    internal class AssetContext : IAssetContext
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

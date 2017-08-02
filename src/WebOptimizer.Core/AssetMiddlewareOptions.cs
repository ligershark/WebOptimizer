using Microsoft.AspNetCore.Hosting;

namespace WebOptimizer
{
    /// <summary>
    /// Options for the WebOptimizer middleware
    /// </summary>
    /// <seealso cref="WebOptimizer.IAssetMiddlewareOptions" />
    public class AssetMiddlewareOptions : IAssetMiddlewareOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetMiddlewareOptions"/> class.
        /// </summary>
        /// <param name="env">The env.</param>
        public AssetMiddlewareOptions(IHostingEnvironment env)
        {
            EnableCaching = !env.IsDevelopment();
        }

        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        public bool? EnableCaching { get; set; }
    }
}

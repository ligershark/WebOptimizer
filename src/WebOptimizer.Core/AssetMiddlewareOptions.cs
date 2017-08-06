using System;
using Microsoft.AspNetCore.Hosting;

namespace WebOptimizer
{
    /// <summary>
    /// Options for the WebOptimizer middleware
    /// </summary>
    /// <seealso cref="WebOptimizer.IAssetMiddlewareOptions" />
    internal class AssetMiddlewareOptions : IAssetMiddlewareOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetMiddlewareOptions"/> class.
        /// </summary>
        /// <param name="env">The env.</param>
        public AssetMiddlewareOptions(IHostingEnvironment env)
        {
            EnableCaching = !env.IsDevelopment();
            SlidingExpiration = TimeSpan.FromHours(24);
        }

        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        public bool? EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets the time from last access to an asset until it is evicted from the cache.
        /// Default it 24 hours.
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; }
    }
}

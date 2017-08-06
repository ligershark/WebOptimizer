using System;

namespace WebOptimizer
{
    /// <summary>
    /// Options for the WebOptimizer middleware
    /// </summary>
    public interface IAssetMiddlewareOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        bool? EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets the time from last access to an asset until it is evicted from the cache.
        /// </summary>
        TimeSpan SlidingExpiration { get; set; }
    }
}
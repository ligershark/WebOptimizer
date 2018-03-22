using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer
{
    /// <summary>
    /// Options for the Web Optimizer.
    /// </summary>
    public interface IWebOptimizerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        bool? EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="IMemoryCache"/> based caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        bool? EnableMemoryCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether disk based caching is enabled.
        /// Default is <code>false</code> when running in a development environment; 
        /// otherwise the default is <code>true</code>.
        /// </summary>
        bool? EnableDiskCache { get; set; }

        /// <summary>
        /// Gets or sets the directory where assets will be stored if <see cref="EnableDiskCache"/> is <code>true</code>.
        /// </summary>
        string CacheDirectory { get; set; }

        /// <summary>
        /// Gets or sets whether bundling is enabled.
        /// </summary>
        bool? EnableTagHelperBundling { get; set; }

        /// <summary>
        /// Gets the CDN url used for TagHelpers.
        /// </summary>
        string CdnUrl { get; }
    }
}
using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer
{
    /// <summary>
    /// Options for the Web Optimizer.
    /// </summary>
    public class WebOptimizerOptions : IWebOptimizerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled.
        /// Default is <code>false</code> when running in a development environment.
        /// </summary>
        public bool? EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="IMemoryCache"/> based caching is enabled.
        /// Default is <code>true</code>.
        /// </summary>
        public bool? EnableMemoryCache { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether disk based caching is enabled.
        /// Default is <code>false</code> when running in a development environment; 
        /// otherwise the default is <code>true</code>.
        /// </summary>
        public bool? EnableDiskCache { get; set; }

        /// <summary>
        /// Gets or sets whether bundling is enabled.
        /// </summary>
        public bool? EnableTagHelperBundling { get; set; }

        /// <summary>
        /// Gets the CDN url used for TagHelpers.
        /// </summary>
        public string CdnUrl { get; set; }

        /// <summary>
        /// Gets or sets the directory where assets will be stored if <see cref="EnableDiskCache"/> is <code>true</code>.
        /// </summary>
        public string CacheDirectory { get; set; }

        /// <summary>
        /// Gets or sets whether empty bundle is allowed to generate instead of throwing an exception
        /// </summary>
        public bool? AllowEmptyBundle { get; set; }
    }
}

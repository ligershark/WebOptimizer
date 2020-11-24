using Microsoft.AspNetCore.Http.Features;
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

        /// <summary>
        /// Gets or sets whether empty bundle is allowed to generate instead of throwing an exception
        /// </summary>
        public bool? AllowEmptyBundle { get; set; }

        /// <summary>
        /// Indicates if files should be compressed for HTTPS requests when the Response Compression middleware is available.
        /// The default value is <see cref="HttpsCompressionMode.Compress"/>.
        /// </summary>
        /// <remarks>
        /// Enabling compression on HTTPS requests for remotely manipulable content may expose security problems.
        /// </remarks>
        HttpsCompressionMode HttpsCompression { get; set; }
    }
}
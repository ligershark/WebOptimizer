using Microsoft.Extensions.FileProviders;

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
        bool? EnableCaching { get; }

        /// <summary>
        /// Gets or sets whether bundling is enabled.
        /// </summary>
        bool? EnableTagHelperBundling { get; }

        /// <summary>
        /// Controls if a "nonce" attribute on scripts and style elements should be generated.
        /// </summary>
        bool? GenerateNonce { get; }

        /// <summary>
        /// Gets the CDN url used for TagHelpers.
        /// </summary>
        string CdnUrl { get; }
    }
}
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
        /// Gets or sets whether bundling is enabled.
        /// </summary>
        public bool? EnableTagHelperBundling { get; set; }

        /// <summary>
        /// Gets the CDN url used for TagHelpers.
        /// </summary>
        public string CdnUrl { get; set; }
    }
}

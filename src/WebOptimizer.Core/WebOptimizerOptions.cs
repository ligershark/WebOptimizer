using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

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
        /// Gets or sets a value indicating whether to use the content root instead of the webroot.
        /// </summary>
        public bool? UseContentRoot { get; set; }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        public virtual IFileProvider FileProvider { get; private set; }

        /// <summary>
        /// Gets the CDN url used for TagHelpers.
        /// </summary>
        public string CdnUrl { get; set; }

        /// <summary>
        /// Ensures that defaults are set
        /// </summary>
        public void EnsureDefaults(IHostingEnvironment env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            EnableCaching = EnableCaching ?? true;
            EnableTagHelperBundling = EnableTagHelperBundling ?? true;
            UseContentRoot = UseContentRoot ?? false;

            FileProvider = UseContentRoot == true ? env.ContentRootFileProvider : env.WebRootFileProvider;
        }
    }
}

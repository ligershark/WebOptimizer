using System.Collections.Generic;

namespace Bundler
{
    /// <summary>
    /// The web optimization pipeline
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled
        /// </summary>
        bool EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the TagHelpers should bundle or write out
        /// tags for each source file.
        /// </summary>
        bool EnabledBundling { get; set; }

        /// <summary>
        /// Adds an <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IPipeline Add(IAsset asset);

        /// <summary>
        /// Adds an array of <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IPipeline Add(IEnumerable<IAsset> asset);

        /// <summary>
        /// Adds an asset to the optimization pipeline.
        /// </summary>
        /// <param name="route">The route matching for the asset.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css".</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to optimize.</param>
        IAsset Add(string route, string contentType, params string[] sourceFiles);
    }
}
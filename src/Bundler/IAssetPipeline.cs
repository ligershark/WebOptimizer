using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Bundler
{
    /// <summary>
    /// The web optimization pipeline
    /// </summary>
    public interface IAssetPipeline
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
        /// Gets the assets registered on the pipeline.
        /// </summary>
        IReadOnlyList<IAsset> Assets { get; }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Adds an <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IAsset Add(IAsset asset);

        /// <summary>
        /// Adds an array of <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IEnumerable<IAsset> Add(IEnumerable<IAsset> asset);

        /// <summary>
        /// Adds an asset to the optimization pipeline.
        /// </summary>
        /// <param name="route">The route matching for the asset.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css".</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to optimize.</param>
        IAsset Add(string route, string contentType, params string[] sourceFiles);

        /// <summary>
        /// Gets the <see cref="IAsset"/> from the specified route.
        /// </summary>
        /// <param name="route">The route to find the asset by.</param>
        IAsset FromRoute(string route);
    }
}
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace WebOptimizer
{
    /// <summary>
    /// The web optimization pipeline
    /// </summary>
    public interface IAssetPipeline
    {
        /// <summary>
        /// Gets the assets registered on the pipeline.
        /// </summary>
        IReadOnlyList<IAsset> Assets { get; }

        /// <summary>
        /// Adds an <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IAsset AddBundle(IAsset asset);

        /// <summary>
        /// Adds an array of <see cref="IAsset"/> to the optimization pipeline.
        /// </summary>
        IEnumerable<IAsset> AddBundle(IEnumerable<IAsset> asset);

        /// <summary>
        /// Adds an asset to the optimization pipeline.
        /// </summary>
        /// <param name="route">The route matching for the asset.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css".</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to optimize.</param>
        IAsset AddBundle(string route, string contentType, params string[] sourceFiles);

        /// <summary>
        /// Add a generalized Asset, that has just a Route and a ContentType
        /// 
        /// Typically used to fill in with content details later by Processors
        /// 
        /// Used by AddJavaScriptBundle() to emit Source Maps on their own route
        /// </summary>
        /// <param name="route">The route that should cause the pipeline to respond with this Asset</param>
        /// <param name="contentType">Content-Type of the response</param>
        IAsset AddAsset(string route, string contentType);

        /// <summary>
        /// Adds an array of files to the optimization pipeline.
        /// </summary>
        /// <param name="contentType">The content type of the response. Example: text/css or text/javascript.</param>
        /// <param name="sourceFiles">A list of relative file names or globbing patterns of the sources to add.</param>
        IEnumerable<IAsset> AddFiles(string contentType, params string[] sourceFiles);

        /// <summary>
        /// Gets the <see cref="IAsset"/> from the specified route.
        /// </summary>
        /// <param name="route">The route to find the asset by.</param>
        /// <param name="asset">The asset matching the route.</param>
        bool TryGetAssetFromRoute(string route, out IAsset asset);
    }
}
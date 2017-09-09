using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer
{
    /// <summary>
    /// An interface for describing a bundle.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets a list of processors
        /// </summary>
        IList<IProcessor> Processors { get; }

        /// <summary>
        /// Gets the items collection for the asset.
        /// </summary>
        IDictionary<string, object> Items { get; }

        /// <summary>
        /// Gets the route to the bundle output.
        /// </summary>
        string Route { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        IEnumerable<string> SourceFiles { get; }

        /// <summary>
        /// Gets the pipeline containing the asset.
        /// </summary>
        IAssetPipeline Pipeline { get; }

        /// <summary>
        /// Executes the processors and returns the modified content.
        /// </summary>
        Task<byte[]> ExecuteAsync(HttpContext context, IWebOptimizerOptions options);

        /// <summary>
        /// Gets the cache key associated with this version of the asset.
        /// </summary>
        string GenerateCacheKey(HttpContext context);
    }
}
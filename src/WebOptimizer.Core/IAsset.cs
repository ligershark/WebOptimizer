using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        /// Gets files to exclude from output results
        /// </summary>
        IList<string> ExcludeFiles { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        HashSet<string> SourceFiles { get; }

        /// <summary>
        /// Executes the processors and returns the modified content.
        /// </summary>
        Task<byte[]> ExecuteAsync(HttpContext context, IWebOptimizerOptions options);

        /// <summary>
        /// Gets the cache key associated with this version of the asset.
        /// </summary>
        string GenerateCacheKey(HttpContext context, IWebOptimizerOptions options);

        /// <summary>
        /// Adds a source file to the asset
        /// </summary>
        /// <param name="route">Relative path of a source file</param>
        void TryAddSourceFile(string route);
    }
}
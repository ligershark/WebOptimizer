using System.Collections.Generic;
using Bundler.Processors;

namespace Bundler
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
        /// Gets the route to the bundle output.
        /// </summary>
        string Route { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        IEnumerable<string> SourceFiles { get; }
    }
}
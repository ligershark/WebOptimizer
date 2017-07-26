using System;
using System.Collections.Generic;

namespace Bundler
{
    /// <summary>
    /// An interface for describing a bundle.
    /// </summary>
    public interface IBundle
    {
        /// <summary>
        /// Gets or sets the cache key.
        /// Append any additional keys to the string in order to vary the cache result
        /// </summary>
        IList<string> QueryKeys { get; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        IList<Action<BundleContext>> PostProcessors { get; }

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
using System;
using System.Collections.Generic;
using Bundler.Processors;

namespace Bundler
{
    /// <summary>
    /// An interface for describing a bundle.
    /// </summary>
    public interface IBundle
    {
        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        IList<IProcessor> PostProcessors { get; }

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
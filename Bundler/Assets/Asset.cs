using System;
using System.Collections.Generic;
using Bundler.Processors;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    internal class Asset : IAsset
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Asset"/> class.
        /// </summary>
        protected Asset()
        { }

        /// <summary>
        /// Gets the route to the bundle output.
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        public IEnumerable<string> SourceFiles { get; internal set; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        public IList<IProcessor> PostProcessors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        public static IAsset Create(string route, string contentType, IEnumerable<string> sourceFiles)
        {
            var bundle = new Asset
            {
                Route = route,
                ContentType = contentType,
                SourceFiles = sourceFiles,
                PostProcessors = new List<IProcessor>(),
            };

            return bundle;
        }
    }
}

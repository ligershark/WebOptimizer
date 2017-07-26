using System;
using System.Collections.Generic;
using Bundler.Processors;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class Bundle : IBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        public Bundle(string route, string contentType, params string[] sourceFiles)
        {
            if (string.IsNullOrEmpty(route) || !route.StartsWith('/'))
            {
                throw new ArgumentException("Path must start with a /", nameof(route));
            }

            Route = route;
            ContentType = contentType;
            SourceFiles = sourceFiles;
            PostProcessors = new List<IProcessor>();
        }

        /// <summary>
        /// Gets the route to the bundle output.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        public IEnumerable<string> SourceFiles { get; internal set; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        public IList<IProcessor> PostProcessors { get; }
    }
}

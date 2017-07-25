using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace Bundler.Transformers
{
    /// <summary>
    /// An interface to describe a content transform
    /// </summary>
    public interface ITransform
    {
        /// <summary>
        /// Gets the route path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        IEnumerable<string> SourceFiles { get; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets or sets the cache key.
        /// Append any additional keys to the string in order to vary the cache result
        /// </summary>
        string CacheKey { get; set; }

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        string Transform(HttpContext context, string source);

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        IList<Func<BundlerConfig, string>> PostProcessors { get; }

        /// <summary>
        /// Includes the specified source files.
        /// </summary>
        ITransform Include(params string[] sourceFiles);
    }
}

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
        /// Transforms the specified source.
        /// </summary>
        string Transform(HttpContext context, string source);

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        IList<Func<string, HttpContext, string>> PostProcessors { get; }

        /// <summary>
        /// Includes the specified source files.
        /// </summary>
        ITransform Include(params string[] sourceFiles);
    }
}

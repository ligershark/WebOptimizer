using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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
        /// Includes the specified source files.
        /// </summary>
        ITransform Include(params string[] sourceFiles);
    }
}

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Bundler.Transformers
{
    /// <summary>
    /// A base class for <see cref="ITransform"/> implementations.
    /// </summary>
    public abstract class BaseTransform : ITransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTransform"/> class.
        /// </summary>
        public BaseTransform(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException($"The \"{nameof(path)}\" parameter must be specified and not empty", nameof(path));
            }

            if (!path.StartsWith('/'))
            {
                throw new System.ArgumentException("Path must start with a /", nameof(path));
            }

            Path = path;
        }

        /// <summary>
        /// </summary>
        /// <!-- Badly formed XML comment ignored for member "P:Bundler.Transformers.ITransform.Path" -->
        public string Path { get; }

        /// <summary>
        /// Gets the webroot relative source files.
        /// </summary>
        public IEnumerable<string> SourceFiles { get; internal set; }

        /// <summary>
        /// Gets the content type produced by the transform.
        /// </summary>
        public abstract string ContentType { get; }

        /// <summary>
        /// Includes the specified source files.
        /// </summary>
        public ITransform Include(params string[] sourceFiles)
        {
            SourceFiles = sourceFiles;

            return this;
        }

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        public abstract string Transform(HttpContext context, string source);
    }
}

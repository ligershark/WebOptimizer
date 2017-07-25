using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

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
            PostProcessors = new List<Func<string, HttpContext, string>>();
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
        /// Gets a list of post processors
        /// </summary>
        public IList<Func<string, HttpContext, string>> PostProcessors { get; }

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
        public string Transform(HttpContext context, string source)
        {
            string result = Run(context, source);

            foreach (Func<string, HttpContext, string> processor in PostProcessors)
            {
                result = processor(result, context);
            }

            return result;
        }

        /// <summary>
        /// Runs the transform on the source.
        /// </summary>
        protected abstract string Run(HttpContext context, string source);
    }
}

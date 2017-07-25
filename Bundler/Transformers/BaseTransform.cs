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
                throw new ArgumentException($"The \"{nameof(path)}\" parameter must be specified and not empty", nameof(path));
            }

            if (!path.StartsWith('/'))
            {
                throw new ArgumentException("Path must start with a /", nameof(path));
            }

            Path = path;
            PostProcessors = new List<Func<BundlerConfig, string>>();
            CacheKeys = new Dictionary<string, string>();
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
        /// Gets or sets the cache key.
        /// Append any additional keys to the string in order to vary the cache result
        /// </summary>
        public IDictionary<string, string> CacheKeys { get; }

        /// <summary>
        /// Gets a list of post processors
        /// </summary>
        public IList<Func<BundlerConfig, string>> PostProcessors { get; }

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
            var config = new BundlerConfig(context, this)
            {
                Content = Run(context, source)
            };

            foreach (Func<BundlerConfig, string> processor in PostProcessors)
            {
                config.Content = processor(config);
            }

            return config.Content;
        }

        /// <summary>
        /// Runs the transform on the source.
        /// </summary>
        protected abstract string Run(HttpContext context, string source);
    }
}

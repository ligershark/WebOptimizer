using System.Collections.Generic;

namespace Bundler
{
    /// <summary>
    /// Options for the bundler transform.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Options"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets a list of transforms added.
        /// </summary>
        internal List<IBundle> Bundles { get; } = new List<IBundle>();

        /// <summary>
        /// Adds a bundle to the middleware pipeline.
        /// </summary>
        public IBundle Add(IBundle bundle)
        {
            Bundles.Add(bundle);

            return bundle;
        }

        /// <summary>
        /// Adds a bundle to the middleware pipeline.
        /// </summary>
        public IBundle Add(string route, string contentType, params string[] sourceFiles)
        {
            var bundle = new Bundle(route, contentType, sourceFiles);
            Bundles.Add(bundle);

            return bundle;
        }
    }
}

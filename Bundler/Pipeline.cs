using System.Collections.Generic;

namespace Bundler
{
    /// <summary>
    /// Options for the bundler transform.
    /// </summary>
    public class Pipeline
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Pipeline"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets a list of transforms added.
        /// </summary>
        internal List<IAsset> Assets { get; } = new List<IAsset>();

        /// <summary>
        /// Adds a bundle to the middleware pipeline.
        /// </summary>
        public Pipeline Add(IAsset asset)
        {
            Assets.Add(asset);

            return this;
        }

        /// <summary>
        /// Adds a list of assets to the pipeline.
        /// </summary>
        public Pipeline Add(IEnumerable<IAsset> asset)
        {
            Assets.AddRange(asset);

            return this;
        }

        /// <summary>
        /// Adds a bundle to the middleware pipeline.
        /// </summary>
        public IAsset Add(string route, string contentType, params string[] sourceFiles)
        {
            string[] sources = sourceFiles;

            if (sourceFiles.Length == 0)
            {
                sources = new[] { route };
            }

            IAsset asset = Asset.Create(route, contentType, sources);
            Assets.Add(asset);

            return asset;
        }
    }
}

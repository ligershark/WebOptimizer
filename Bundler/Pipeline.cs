using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Bundler
{
    /// <summary>
    /// Options for the bundler transform.
    /// </summary>
    public class Pipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class.
        /// </summary>
        public Pipeline(IHostingEnvironment env)
        {
            EnableCaching = !env.IsDevelopment();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the TagHelpers should bundle or write out
        /// tags for each source file.
        /// </summary>
        public bool EnabledBundling { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether server-side caching is enabled
        /// </summary>
        public bool EnableCaching { get; set; }

        /// <summary>
        /// Adds a bundle to the middleware pipeline.
        /// </summary>
        public Pipeline Add(IAsset asset)
        {
            AssetManager.Assets.Add(asset);

            return this;
        }

        /// <summary>
        /// Adds a list of assets to the pipeline.
        /// </summary>
        public Pipeline Add(IEnumerable<IAsset> asset)
        {
            AssetManager.Assets.AddRange(asset);

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
            AssetManager.Assets.Add(asset);

            return asset;
        }

        /// <summary>
        /// Adds a list of assets to the pipeline.
        /// </summary>
        public IEnumerable<IAsset> AddFiles(string contentType, params string[] sourceFiles)
        {
            return sourceFiles.Select(f => Add(f, contentType)).ToArray();
        }
    }
}

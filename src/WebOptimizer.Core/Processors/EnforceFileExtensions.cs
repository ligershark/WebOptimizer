using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebOptimizer
{
    internal class EnforceFileExtensions : Processor
    {
        private IEnumerable<string> _extensions;

        public EnforceFileExtensions(IEnumerable<string> extensions)
        {
            _extensions = extensions;
        }

        public override Task ExecuteAsync(IAssetContext context)
        {
            foreach (string file in context.Content.Keys)
            {
                string ext = Path.GetExtension(file);

                if (!_extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new NotSupportedException($"The file extension \"{ext}\" is not valid for this asset");
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Throws an exception if any file doesn't match one of the specified file extensions.
        /// </summary>
        public static IAsset EnforceFileExtensions(this IAsset asset, params string[] extensions)
        {
            var processor = new EnforceFileExtensions(extensions);
            asset.Processors.Add(processor);

            return asset;
        }

        /// <summary>
        /// Throws an exception if any file doesn't match one of the specified file extensions.
        /// </summary>
        public static IEnumerable<IAsset> EnforceFileExtensions(this IEnumerable<IAsset> assets, params string[] extensions)
        {
            return assets.AddProcessor(asset => asset.EnforceFileExtensions());
        }
    }
}

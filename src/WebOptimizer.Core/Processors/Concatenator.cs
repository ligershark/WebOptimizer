using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebOptimizer;

namespace WebOptimizer
{
    internal class Concatenator : Processor
    {
        public override Task ExecuteAsync(IAssetContext context)
        {
            // If there is no content, nothing to do
            if (context.Content.Count > 0)
            {
                var sb = new StringBuilder();

                foreach (byte[] bytes in context.Content.Values)
                {
                    sb.AppendLine(bytes.AsString());
                }

                // Use existing first key as new key to have a valid input for subsequent calls to GetFileInfo
                var newKey = context.Content.Keys.First();
                context.Content = new Dictionary<string, byte[]>
                {
                    { newKey, sb.ToString().AsByteArray() }
                };
            }
            return Task.CompletedTask;
        }
    }

}
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Adds the string content of all source files to the pipeline.
        /// </summary>
        public static IAsset Concatenate(this IAsset asset)
        {
            var reader = new Concatenator();
            asset.Processors.Add(reader);

            return asset;
        }

        /// <summary>
        /// Adds the string content of all source files to the pipeline.
        /// </summary>
        public static IEnumerable<IAsset> Concatenate(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.Concatenate());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebOptimizer
{
    internal class Concatenator : Processor
    {
        public override Task ExecuteAsync(IAssetContext context)
        {
            var sb = new StringBuilder();

            foreach (byte[] bytes in context.Content.Values)
            {
                sb.AppendLine(bytes.AsString());
            }

            context.Content = new Dictionary<string, byte[]>
            {
                { Guid.NewGuid().ToString(), sb.ToString().AsByteArray() }
            };

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
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

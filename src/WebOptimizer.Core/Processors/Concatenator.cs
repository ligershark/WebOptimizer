using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WebOptimizer;

namespace WebOptimizer
{
    internal partial class Concatenator : Processor
    {
        [GeneratedRegex(@"(?i:.min.(css|js|html)$)")]
        private static partial Regex MinifiedFileRegex();

        private static readonly Regex _minifiedFileRegex = MinifiedFileRegex();
        public override Task ExecuteAsync(IAssetContext context)
        {
            var sb = new StringBuilder();

            foreach (byte[] bytes in context.Content.Values)
            {
                sb.AppendLine(bytes.AsString());
            }

            // Use Guid as key and append .min if all the included files seem to be minified
            var newKey = Guid.NewGuid().ToString();

            if (context.Content.Keys.All(k => _minifiedFileRegex.IsMatch(k)))
            {
                newKey += ".min";
            }
            context.Content = new Dictionary<string, byte[]>
            {
                { newKey, sb.ToString().AsByteArray() }
            };
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
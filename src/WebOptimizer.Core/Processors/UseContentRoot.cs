using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;

namespace WebOptimizer
{
    internal class UseContentRoot : Processor
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
        public static IAsset UseContentRoot(this IAsset asset)
        {
            asset.Items["usecontentroot"] = true;

            return asset;
        }

        internal static bool IsUsingContentRoot(this IAsset asset)
        {
            if (asset?.Items == null)
            {
                return false;
            }

            if (asset.Items.TryGetValue("usecontentroot", out var value) && value is bool useContentRoot)
            {
                return useContentRoot;
            }

            return false;
        }
    }
}

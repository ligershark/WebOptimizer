using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebOptimizer
{
    /// <summary>
    /// Concatinates multiple files into a single string.
    /// </summary>
    internal class Concatinator : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context) => string.Empty;

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();

                foreach (string content in context.Content.Values)
                {
                    sb.AppendLine(content);
                }

                context.Content = new Dictionary<string, string>
                {
                    { Guid.NewGuid().ToString(), sb.ToString() }
                };
            });
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
        public static IAsset Concatinate(this IAsset asset)
        {
            var reader = new Concatinator();
            asset.Processors.Add(reader);

            return asset;
        }

        /// <summary>
        /// Adds the string content of all source files to the pipeline.
        /// </summary>
        public static IEnumerable<IAsset> Concatinate(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.Concatinate());
            }

            return list;
        }
    }
}

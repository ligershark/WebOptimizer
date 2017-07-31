using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Bundler
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
        public async Task ExecuteAsync(IAssetContext context)
        {
            IFileProvider fileProvider = AssetManager.Pipeline.FileProvider;
            IEnumerable<string> absolutes = context.Asset.SourceFiles.Select(f => fileProvider.GetFileInfo(f).PhysicalPath);
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                using (var fs = new FileStream(absolute, FileMode.Open))
                using (var reader = new StreamReader(fs))
                {
                    sb.AppendLine(await reader.ReadToEndAsync());
                }
            }

            context.Content = sb.ToString();
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class ConcatinatorExtensions
    {
        /// <summary>
        /// Runs the bundler on the content.
        /// </summary>
        public static IAsset Bundle(this IAsset asset)
        {
            var bundler = new Concatinator();
            asset.Processors.Add(bundler);

            return asset;
        }

        /// <summary>
        /// Runs the bundler on the content.
        /// </summary>
        public static IEnumerable<IAsset> Bundle(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.Bundle());
            }

            return list;
        }
    }
}

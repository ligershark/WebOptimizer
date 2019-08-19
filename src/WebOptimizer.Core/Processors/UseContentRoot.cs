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
        /// Uses the content root folder (usually the project root) instead of the wwwroot.
        /// </summary>
        public static IAsset UseContentRoot(this IAsset asset)
        {
            asset.Items["usecontentroot"] = true;

            return asset;
        }

        /// <summary>
        /// Uses the specified <see cref="IFileProvider"/> to locate the source files.
        /// </summary>
        public static IAsset UseFileProvider(this IAsset asset, IFileProvider fileProvider)
        {
            asset.Items["fileprovider"] = fileProvider;

            return asset;
        }

        internal static IFileProvider GetCustomFileProvider(this IAsset asset, IWebHostEnvironment env)
        {
            if (asset?.Items == null)
            {
                return null;
            }

            if (asset.Items.TryGetValue("usecontentroot", out object value) && value is bool useContentRoot)
            {
                return env.ContentRootFileProvider;
            }

            if (asset.Items.TryGetValue("fileprovider", out value) && value is IFileProvider provider)
            {
                return provider;
            }

            return null;
        }
    }
}

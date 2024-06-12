using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebOptimizer;
using WebOptimizer.Processors;

namespace WebOptimizer.Processors
{
    internal class ItemContentEmitter : Processor
    {
        public override Task ExecuteAsync(IAssetContext context)
        {
            var asset = context.Asset;
            var items = asset.Items;
            if (!items.ContainsKey("Content"))
                return Task.CompletedTask;

            context.Content = new Dictionary<string, byte[]>
            {
                { "Content", ((string)items["Content"]).AsByteArray() }
            };

            return Task.CompletedTask;
        }
    }
}


namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Changes the Asset to only emit to Response what is stored in
        /// asset.Items["Content"]
        /// and nothing else
        /// 
        /// Useful for Generated Content
        /// 
        /// Used by JavaScriptMinifier.AddJavaScriptBundle to emit sourcemaps into a separate Asset
        /// when generating minified code
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static IAsset UseItemContent(this IAsset asset)
        {
            asset.Processors.Add(new ItemContentEmitter());
            return asset;
        }

        /// <summary>
        /// Changes the Asset to only emit to Response what is stored in
        /// asset.Items["Content"]
        /// and nothing else
        /// 
        /// Useful for Generated Content
        /// 
        /// Used by JavaScriptMinifier.AddJavaScriptBundle to emit sourcemaps into a separate Asset
        /// when generating minified code
        /// </summary>
        public static IEnumerable<IAsset> UseItemContent(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.UseItemContent());
        }
    }
}

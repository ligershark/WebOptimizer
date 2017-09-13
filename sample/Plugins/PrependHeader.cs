using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebOptimizer;
using WebOptimizerDemo;

namespace WebOptimizerDemo
{
    public class PrependHeader : Processor
    {
        private string _header;

        public PrependHeader(string header)
        {
            _header = header;
        }

        public override Task ExecuteAsync(IAssetContext context)
        {
            var content = new Dictionary<string, byte[]>();
            string header = $"/*\r\n   {_header}\r\n*/";

            foreach (string route in context.Content.Keys)
            {
                string update = header + Environment.NewLine + context.Content[route].AsString();
                content[route] = update.AsByteArray();
            }

            context.Content = content;

            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AssetPipelinerExtensions
    {
        public static IAsset PrependHeader(this IAsset asset, string header)
        {
            asset.Processors.Add(new PrependHeader(header));
            return asset;
        }

        public static IEnumerable<IAsset> PrependHeader(this IEnumerable<IAsset> assets, string header)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.PrependHeader(header));
            }

            return list;
        }
    }
}
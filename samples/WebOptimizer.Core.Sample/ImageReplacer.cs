using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;

namespace BundlerSample
{
    public class ImageReplacer : IProcessor
    {
        public string CacheKey(HttpContext context) => string.Empty;

        public async Task ExecuteAsync(IAssetContext context, WebOptimizerOptions options)
        {
            var pipeline = (IAssetPipeline)context.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
            var content = new Dictionary<string, byte[]>();
            IFileInfo file = options.FileProvider.GetFileInfo("/images/logo.png");

            using (Stream fs = file.CreateReadStream())
            {
                byte[] bytes = await fs.AsBytesAsync();

                foreach (string route in context.Content.Keys)
                {
                    content[route] = bytes;
                }
            }

            context.Content = content;
        }
    }

    public static class ImageReplacerExtensions
    {
        public static IAsset ReplaceImages(this IAsset asset)
        {
            asset.Processors.Add(new ImageReplacer());
            return asset;
        }

        public static IEnumerable<IAsset> ReplaceImages(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.ReplaceImages());
            }

            return list;
        }

        public static IAsset AddImageReplacer(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "image/png", sourceFiles)
                           .ReplaceImages();
        }

        public static IEnumerable<IAsset> ReplaceImages(this IAssetPipeline pipeline)
        {
            return pipeline.AddFiles("image/png", "**/*.png")
                           .ReplaceImages();
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SharpScss;
using WebOptimizer;

namespace BundlerSample
{
    public class ScssCompiler : IProcessor
    {
        public string CacheKey(HttpContext context) => string.Empty;

        public Task ExecuteAsync(IAssetContext context)
        {
            return Task.Run(() =>
            {
                var pipeline = (IAssetPipeline)context.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
                var content = new Dictionary<string, string>();

                foreach (string route in context.Content.Keys)
                {
                    IFileInfo file = pipeline.FileProvider.GetFileInfo(route);
                    var options = new ScssOptions { InputFile = file.PhysicalPath };

                    ScssResult result = Scss.ConvertToCss(context.Content[route], options);

                    content[route] = result.Css;
                }

                context.Content = content;
            });
        }
    }

    public static class ScssCompilerExtensions
    {
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.Processors.Add(new ScssCompiler());
            return asset;
        }

        public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.CompileScss();
            }
        }

        public static IAsset AddScss(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .CompileScss()
                           .Concatinate();
        }
    }
}
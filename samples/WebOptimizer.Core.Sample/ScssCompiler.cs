using System.Collections.Generic;
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

            return Task.CompletedTask;
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
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.CompileScss());
            }

            return list;
        }

        public static IAsset AddScss(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .CompileScss()
                           .AdjustRelativePaths()
                           .Concatinate()
                           .CssFingerprint()
                           .MinifyCss();
        }

        public static IAsset AddScss(this IAssetPipeline pipeline)
        {
            return pipeline.AddFileExtension(".scss", "text/css")
                           .CompileScss()
                           .AdjustRelativePaths()
                           .CssFingerprint()
                           .MinifyCss();
        }
    }
}
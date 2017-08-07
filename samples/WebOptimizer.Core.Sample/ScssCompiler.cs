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
            var content = new Dictionary<string, byte[]>();

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = pipeline.FileProvider.GetFileInfo(route);
                var options = new ScssOptions { InputFile = file.PhysicalPath };

                ScssResult result = Scss.ConvertToCss(context.Content[route].AsString(), options);

                content[route] = result.Css.AsByteArray();
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

        public static IAsset AddScssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                           .CompileScss()
                           .AdjustRelativePaths()
                           .Concatenate()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", "**/*.scss")
                           .CompileScss()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/css; charset=UFT-8", sourceFiles)
                           .CompileScss()
                           .FingerprintUrls()
                           .MinifyCss();
        }
    }
}
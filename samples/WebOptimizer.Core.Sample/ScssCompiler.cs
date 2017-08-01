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
        private IEnumerable<string> _routes;

        public ScssCompiler(IEnumerable<string> routes)
        {
            _routes = routes;
        }

        public string CacheKey(HttpContext context) => string.Empty;

        public async Task ExecuteAsync(IAssetContext context)
        {
            var pipeline = (IAssetPipeline)context.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
            var sb = new StringBuilder();

            foreach (string route in _routes)
            {
                IFileInfo file = pipeline.FileProvider.GetFileInfo(route);
                var options = new ScssOptions { InputFile = file.PhysicalPath };

                using (var reader = new StreamReader(file.PhysicalPath))
                {
                    string source = await reader.ReadToEndAsync();
                    ScssResult result = Scss.ConvertToCss(source, options);

                    sb.AppendLine(result.Css);
                }
            }

            context.Content = sb.ToString();
        }
    }

    public static class ScssCompilerExtensions
    {
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.Processors.Add(new ScssCompiler(asset.SourceFiles));
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
                           .ReadFromDisk()
                           .CompileScss();
        }
    }
}

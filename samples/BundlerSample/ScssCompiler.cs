using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bundler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SharpScss;

namespace BundlerSample
{
    public class ScssCompiler : IProcessor
    {
        private IEnumerable<string> _routes;

        public ScssCompiler(IEnumerable<string> route)
        {
            _routes = route;
        }

        public string CacheKey(HttpContext context) => string.Empty;

        public Task ExecuteAsync(IAssetContext context)
        {
            return Task.Run(() =>
            {
                var options = new ScssOptions();

                foreach (string route in _routes)
                {
                    IFileInfo file = AssetManager.Environment.WebRootFileProvider.GetFileInfo(route);
                    string dir = Path.GetDirectoryName(file.PhysicalPath);

                    if (!options.IncludePaths.Contains(dir))
                    {
                        options.IncludePaths.Add(dir);
                    }
                }

                context.Content = Scss.ConvertToCss(context.Content, options).Css;
            });
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

        public static IAsset AddScss(this IPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .Bundle()
                           .CompileScss();
        }
    }
}

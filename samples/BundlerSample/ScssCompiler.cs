using System.Collections.Generic;
using System.IO;
using Bundler;
using Bundler.Processors;
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

        public void Execute(IAssetContext context)
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
        }
    }

    public static class ScssCompilerExtensions
    {
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.PostProcessors.Add(new ScssCompiler(asset.SourceFiles));
            return asset;
        }

        public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets)
        {
            foreach (IAsset asset in assets)
            {
                yield return asset.CompileScss();
            }
        }

        public static IAsset AddScss(this Pipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .CompileScss();
        }
    }
}

using System.Collections.Generic;
using Bundler;
using Bundler.Processors;
using Microsoft.AspNetCore.Http;
using SharpScss;

namespace BundlerSample
{
    public class ScssCompiler : IProcessor
    {
        public string CacheKey(HttpContext context) => string.Empty;

        public void Execute(IAssetContext context)
        {
            context.Content = Scss.ConvertToCss(context.Content).Css;
        }
    }

    public static class ScssCompilerExtensions
    {
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.PostProcessors.Add(new ScssCompiler());
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

using Bundler.Transformers;
using Microsoft.AspNetCore.Builder;
using NUglify.Css;
using NUglify.JavaScript;
using System;

namespace Bundler
{
    public static class Extensions
    {
        // TODO: Add this to DI
        public static Options Options { get; } = new Options();

        public static void UseTransforms(this IApplicationBuilder app, Action<Options> transformOptions)
        {
            transformOptions(Options);

            foreach (var transform in Options.Transforms)
            {
                app.Map(transform.Path, builder => {
                    builder.UseMiddleware<TransformMiddleware>(transform);
                });
            }
        }

        public static void AddJs(this Options options, string route, params string[] sourceFiles)
        {
            options.Transforms.Add(new JavaScriptMinifier(route).Include(sourceFiles));
        }

        public static void AddJs(this Options options, CodeSettings settings, string route, params string[] sourceFiles)
        {
            options.Transforms.Add(new JavaScriptMinifier(route, settings).Include(sourceFiles));
        }

        public static void AddCss(this Options options, string route, params string[] sourceFiles)
        {
            options.Transforms.Add(new CssMinifier(route).Include(sourceFiles));
        }

        public static void AddCss(this Options options, CssSettings settings, string route, params string[] sourceFiles)
        {
            options.Transforms.Add(new CssMinifier(route, settings).Include(sourceFiles));
        }
    }
}

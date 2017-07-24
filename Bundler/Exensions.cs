using Microsoft.AspNetCore.Builder;
using System;

namespace Bundler
{
    public static class Exensions
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
    }
}

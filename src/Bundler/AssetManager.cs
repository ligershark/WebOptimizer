using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Bundler
{
    /// <summary>
    /// Contains the state of the assets.
    /// </summary>
    public static class AssetManager
    {
        /// <summary>
        /// Gets a list of transforms added.
        /// </summary>
        internal static List<IAsset> Assets { get; } = new List<IAsset>();

        /// <summary>
        /// Gets the asset pipeline configuration
        /// </summary>
        internal static Pipeline Pipeline { get; set; }

        /// <summary>
        /// Gets the builder associated with the pipeline.
        /// </summary>
        internal static IApplicationBuilder Builder { get; set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public static IHostingEnvironment Environment { get; internal set; }

        /// <summary>
        /// Adds Bundler to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static void UseWebOptimizer(this IApplicationBuilder app, IHostingEnvironment env, Action<Pipeline> assetPipeline)
        {
            Environment = env;
            Builder = app;
            Pipeline = new Pipeline(env);

            assetPipeline(Pipeline);

            AssetMiddleware mw = ActivatorUtilities.CreateInstance<AssetMiddleware>(app.ApplicationServices);

            app.UseRouter(routes =>
            {
                foreach (IAsset asset in Assets)
                {
                    routes.MapGet(asset.Route, context => mw.InvokeAsync(context, asset));
                }
            });
        }
    }
}

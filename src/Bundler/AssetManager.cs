using System;
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
        /// This is only used by the Localizer. Would be great not to have it here if possible
        /// </summary>
        internal static IApplicationBuilder Builder { get; private set; }

        /// <summary>
        /// Gets the asset pipeline configuration
        /// </summary>
        public static IAssetPipeline Pipeline { get; private set; }

        /// <summary>
        /// Adds WebOptimizer to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static void UseWebOptimizer(this IApplicationBuilder app, IHostingEnvironment env, Action<IAssetPipeline> assetPipeline)
        {
            Builder = app;
            Pipeline = new AssetPipeline(env);

            assetPipeline(Pipeline);

            AssetMiddleware mw = ActivatorUtilities.CreateInstance<AssetMiddleware>(app.ApplicationServices);

            app.UseRouter(routes =>
            {
                foreach (IAsset asset in Pipeline.Assets)
                {
                    routes.MapGet(asset.Route, context => mw.InvokeAsync(context, asset));
                }
            });
        }
    }
}

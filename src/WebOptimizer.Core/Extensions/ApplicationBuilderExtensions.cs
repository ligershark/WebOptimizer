using System;
using Microsoft.AspNetCore.Hosting;
using WebOptimizer;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    { 
        /// <summary>
        /// Adds WebOptimizer to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static IApplicationBuilder UseWebOptimizer(this IApplicationBuilder app, IWebHostEnvironment env, FileProviderOptions[] fileProviderOptions = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            
            env.WebRootFileProvider = new CompositeFileProviderExtended(env.WebRootFileProvider, fileProviderOptions);

            app.UseWebOptimizer();

            return app;
        }


        /// <summary>
        /// Adds WebOptimizer to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        public static IApplicationBuilder UseWebOptimizer(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (app.ApplicationServices.GetService(typeof(IAssetPipeline)) == null)
            {
                // TODO: This error message is incorrect for Program.cs in .Net 8.0 - ConfigureServices() is retired and the call is more like services.AddWebOptimizer() now.
                string msg = "Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddWebOptimizer' inside the call to 'ConfigureServices(...)' in the application startup code.";
                throw new InvalidOperationException(msg);
            }

            app.UseMiddleware<AssetMiddleware>();

            return app;
        }
    }
}

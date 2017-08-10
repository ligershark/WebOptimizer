using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;

namespace BundlerSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc()
                .AddViewLocalization(options => options.ResourcesPath = "Resources");

            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle("/all.css", "css/site1.css", "lib/bootstrap/dist/css/bootstraps.css")
                        .InlineImages();

                pipeline.AddJavaScriptBundle("/all.js", "js/site.js", "js/b.js");

                pipeline.AddBundle("/demo.txt", "text/plain", "js/site.js", "js/b.js")
                      .Concatenate();

                pipeline.MinifyJsFiles("**/*.jsx");

                pipeline.AddScssBundle("/scss.css", "css/*.scss");

                pipeline.MinifyCssFiles().InlineImages();
                pipeline.CompileScssFiles();
                pipeline.ReplaceImages();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAssetPipeline pipeline)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var cultures = new List<CultureInfo>
            {
                new CultureInfo("en"),
                new CultureInfo("da")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                SupportedCultures = cultures,
                SupportedUICultures = cultures
            });

            app.UseWebOptimizer();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

}

namespace BundlerSample
{
    public class Strings
    {

    }
}

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
            services.AddMvc()
                .AddViewLocalization(options => options.ResourcesPath = "Resources");

            services.AddWebOptimizer(assets =>
            {
                assets.EnableTagHelperBundling = true;
                assets.AddCss("/all.css", "css/site.css", "lib/bootstrap/dist/css/bootstrap.css");

                assets.AddJs("/all.js", "js/site.js", "js/b.js")
                      .Localize<Strings>();

                assets.Add("/test.js", "application/javascript", "js/site.js", "js/b.js")
                      .Concatinate()
                      .MinifyJavaScript()
                      .Localize<Strings>();

                // These files exist on disk and will now be localized and minified
                //assets.AddFiles("application/javascript", "/js/site.js", "/js/b.js")
                //      .Localize<Strings>()
                //      .MinifyJavaScript(new NUglify.JavaScript.CodeSettings { PreserveImportantComments = false });

                assets.AddCss();
                assets.AddJs();
                assets.AddScss();

                assets.AddScss("/scss.css", "css/test.scss", "css/test2.scss")
                      .Localize<Strings>()
                      .MinifyCss();
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

            pipeline.FileProvider = env.WebRootFileProvider;
            app.UseWebOptimizer(options =>
            {
                //options.EnableCaching = true;
            });

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

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace WebOptimizer.Core.Sample2
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddResponseCompression();

            var cssSettings = new CssBundlingSettings();
            var codeSettings = new CodeBundlingSettings
            {
                Minify = true,
            };

            services.AddWebOptimizer(HostingEnvironment, cssSettings, codeSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseResponseCompression();

            const string scriptsPath1 = "Scripts1";
            const string scriptsPath2 = "Scripts2";

            var currentDirectory = Directory.GetCurrentDirectory();
            app.UseWebOptimizer(HostingEnvironment, new[]
            {
                new FileProviderOptions
                {
                    RequestPath = "/" + scriptsPath1,
                    FileProvider = new PhysicalFileProvider(Path.Combine(currentDirectory, scriptsPath1))
                },
                new FileProviderOptions
                {
                    RequestPath = "/" + scriptsPath2,
                    FileProvider = new PhysicalFileProvider(Path.Combine(currentDirectory, scriptsPath2))
                },
                new FileProviderOptions
                {
                    RequestPath = "/EmbeddedResourcesScripts",
                    FileProvider = new EmbeddedFileProvider(Lib.AssemblyTools.GetCurrentAssembly()),
                }
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

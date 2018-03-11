using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer.Core.Sample2
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();


            var cssSettings = new CssBundlingSettings();
            var codeSettings = new CodeBundlingSettings
            {
                Minify = true,
            };

            services.AddWebOptimizer(HostingEnvironment, cssSettings, codeSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            const string scriptsPath1 = "Scripts1";
            const string scriptsPath2 = "Scripts2";

            var currentDirectory = Directory.GetCurrentDirectory();
            app.UseWebOptimizer(HostingEnvironment, new[]
            {
                new StaticFileOptions
                {
                    RequestPath = "/" + scriptsPath1,
                    FileProvider = new PhysicalFileProvider(Path.Combine(currentDirectory, scriptsPath1))
                },
                new StaticFileOptions
                {
                    RequestPath = "/" + scriptsPath2,
                    FileProvider = new PhysicalFileProvider(Path.Combine(currentDirectory, scriptsPath2))
                },
                new StaticFileOptions
                {
                    RequestPath = "/EmbeddedResourcesScripts",
                    FileProvider = new EmbeddedFileProvider(Lib.AssemblyTools.GetCurrentAssembly()),
                }
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

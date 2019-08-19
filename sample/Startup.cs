using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebOptimizerDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddWebOptimizer(pipeline =>
            {
                // Creates a CSS and a JS bundle. Globbing patterns supported.
                pipeline.AddCssBundle("/css/bundle.css", "css/*.css");
                pipeline.AddJavaScriptBundle("/js/bundle.js", "js/plus.js", "js/minus.js");

                // This bundle uses source files from the Content Root and uses a custom PrependHeader extension
                pipeline.AddJavaScriptBundle("/js/scripts.js", "scripts/a.js", "wwwroot/js/plus.js")
                        .UseContentRoot()
                        .PrependHeader("My custom header")
                        .AddResponseHeader("x-test", "value");

                // This will minify any JS and CSS file that isn't part of any bundle
                pipeline.MinifyCssFiles();
                pipeline.MinifyJsFiles();

                // This will automatically compile any referenced .scss files
                pipeline.CompileScssFiles();

                // AddFiles/AddBundle allow for custom pipelines
                pipeline.AddBundle("/text.txt", "text/plain", "random/*.txt")
                        .AdjustRelativePaths()
                        .Concatenate()
                        .FingerprintUrls()
                        .MinifyCss();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebOptimizer();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}

using System;
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
            services.AddResponseCaching();
            services.AddMvc();

            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle("/all.css", "lib/bootstrap/dist/css/bootstrap.css", "css/site.css")
                        .InlineImages();

                pipeline.AddJavaScriptBundle("/all.js", "js/site.js", "js/b.js");

                pipeline.AddBundle("/demo.txt", "text/plain", "js/site.js", "js/b.js")
                        .Concatenate();

                pipeline.MinifyJsFiles("**/*.jsx");
                pipeline.AddBundle("test.res", "text/xml", "Resources/Strings.resx").UseContentRoot();

                pipeline.AddScssBundle("/scss.css", "css/test2.scss", "css/test.scss");

                pipeline.MinifyCssFiles("css/site.css").InlineImages();
                pipeline.CompileScssFiles();
                pipeline.ReplaceImages();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseWebOptimizer();
            app.UseResponseCaching();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    var expires = TimeSpan.FromDays(365);
                    context.Context.Response.Headers["Cache-Control"] = $"public, max-age={expires.TotalSeconds}";
                    context.Context.Response.Headers["Expires"] = DateTime.Now.Add(expires).ToString("R");
                }
            });

            app.UseETagger();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
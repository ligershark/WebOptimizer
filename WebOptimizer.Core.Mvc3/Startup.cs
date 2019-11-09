using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebOptimizer.Core.Mvc3
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
            services.AddControllersWithViews();
            services.AddWebOptimizer(pipeline =>
            {
                //pipeline.MinifyCssFiles("/css/**/*.css");
                //pipeline.MinifyJsFiles("/js/**/*.js");
                pipeline.AddCssBundle("/css/test/bundle1.css", "css/test/**/**.css");
                pipeline.AddCssBundle("/css/test/bundle2.css", "css/test/a.css", "css/test/b.css");
                pipeline.AddJavaScriptBundle("/js/test/combined1.js", "js/test/**/*.js");
                pipeline.AddJavaScriptBundle("/js/test/combined2.js", "js/test/minus.js","js/test/plus.js");
            });

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
            app.UseWebOptimizer();
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

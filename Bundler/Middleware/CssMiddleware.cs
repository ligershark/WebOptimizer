using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.Css;
using System.IO;
using System.Threading.Tasks;

namespace Bundler
{
    /// <summary>
    /// Middleware for minifying CSS.
    /// </summary>
    public class CssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CssSettings _settings;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="CssMiddleware"/> class.
        /// </summary>
        public CssMiddleware(RequestDelegate next, CssSettings settings, IHostingEnvironment env)
        {
            _next = next;
            _settings = settings;
            _env = env;
        }


        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            string ext = Path.GetExtension(context.Request.Path.Value);

            if (ext != ".css")
            {
                await _next(context);
                return;
            }

            string file = Path.Combine(_env.WebRootPath, context.Request.Path.Value.TrimStart('/'));

            if (File.Exists(file))
            {
                string source = await File.ReadAllTextAsync(file);
                var transform = new CssMinifier(context.Request.Path, _settings);

                string minified = transform.Transform(context, source);

                context.Response.ContentType = transform.ContentType;
                await context.Response.WriteAsync(minified);
            }
            else
            {
                await _next(context);
            }
        }
    }
}

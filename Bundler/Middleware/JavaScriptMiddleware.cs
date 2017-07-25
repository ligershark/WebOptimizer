using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.JavaScript;
using System.IO;
using System.Threading.Tasks;

namespace Bundler
{
    /// <summary>
    /// Middleware for minifying JavaScript
    /// </summary>
    public class JavaScriptMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CodeSettings _settings;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMiddleware"/> class.
        /// </summary>
        public JavaScriptMiddleware(RequestDelegate next, CodeSettings settings, IHostingEnvironment env)
        {
            _next = next;
            _settings = settings;
            _env = env;
        }


        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            string ext = Path.GetExtension(context.Request.Path.Value);

            if (ext != ".js")
            {
                await _next(context);
                return;
            }

            string file = Path.Combine(_env.WebRootPath, context.Request.Path.Value.TrimStart('/'));

            if (File.Exists(file))
            {
                string source = await File.ReadAllTextAsync(file);
                var transform = new JavaScriptMinifier(context.Request.Path, _settings);

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

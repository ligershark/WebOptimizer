using System.IO;
using System.Threading.Tasks;
using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// Middleware for minifying JavaScript
    /// </summary>
    public class JavaScriptMiddleware : BaseMiddleware
    {
        private readonly CodeSettings _settings;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMiddleware"/> class.
        /// </summary>
        public JavaScriptMiddleware(RequestDelegate next, CodeSettings settings, IHostingEnvironment env)
            : base(next)
        {
            _settings = settings;
            _env = env;
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected override string ContentType => "application/javascript";

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public override async Task<string> ExecuteAsync(HttpContext context)
        {
            string ext = Path.GetExtension(context.Request.Path.Value);

            if (ext != ".js")
            {
                return null;
            }

            string file = Path.Combine(_env.WebRootPath, context.Request.Path.Value.TrimStart('/'));

            if (File.Exists(file))
            {
                string source = await File.ReadAllTextAsync(file);
                var transform = new JavaScriptMinifier(context.Request.Path, _settings);

                return transform.Transform(context, source);
            }

            return null;
        }
    }
}

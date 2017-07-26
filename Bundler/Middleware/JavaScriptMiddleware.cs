using System.IO;
using System.Threading.Tasks;
using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// Middleware for minifying JavaScript
    /// </summary>
    public class JavaScriptMiddleware : BaseMiddleware
    {
        private readonly CodeSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMiddleware"/> class.
        /// </summary>
        public JavaScriptMiddleware(RequestDelegate next, CodeSettings settings, IHostingEnvironment env, IMemoryCache cache)
            : base(next, cache, env)
        {
            _settings = settings;
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

            IFileInfo file = FileProvider.GetFileInfo(context.Request.Path.Value);

            if (file.Exists)
            {
                string source = await File.ReadAllTextAsync(file.PhysicalPath);
                var transform = new JavaScriptMinifier(context.Request.Path, _settings);

                return transform.Transform(context, source);
            }

            return null;
        }
    }
}

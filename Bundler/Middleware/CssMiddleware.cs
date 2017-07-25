using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.Css;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Bundler
{
    /// <summary>
    /// Middleware for minifying CSS.
    /// </summary>
    public class CssMiddleware : BaseMiddleware
    {
        private readonly CssSettings _settings;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="CssMiddleware"/> class.
        /// </summary>
        public CssMiddleware(RequestDelegate next, CssSettings settings, IHostingEnvironment env)
          : base(next)
        {
            _settings = settings;
            _env = env;
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        protected override string ContentType => "text/css";

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public override async Task<string> ExecuteAsync(HttpContext context)
        {
            string ext = Path.GetExtension(context.Request.Path.Value);

            if (ext != ".css")
            {
                return null;
            }

            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(context.Request.Path.Value);

            if (file.Exists)
            {
                string source = await File.ReadAllTextAsync(file.PhysicalPath);
                var transform = new CssMinifier(context.Request.Path, _settings);

                return transform.Transform(context, source);
            }

            return null;
        }
    }
}

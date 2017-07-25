using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundler
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    public class BundleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly ITransform _transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BundleMiddleware(RequestDelegate next, IHostingEnvironment env, ITransform transform)
        {
            _next = next;
            _env = env;
            _transform = transform;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            string source = await GetContentAsync(_transform);

            string transformedBundle = _transform.Transform(context, source);

            context.Response.ContentType = _transform.ContentType;
            await context.Response.WriteAsync(transformedBundle);
        }

        private async Task<string> GetContentAsync(ITransform transform)
        {
            System.Collections.Generic.IEnumerable<string> absolutes = transform.SourceFiles.Select(f => Path.Combine(_env.WebRootPath, f));
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                sb.AppendLine(await File.ReadAllTextAsync(absolute));
            }

            return sb.ToString();
        }
    }
}

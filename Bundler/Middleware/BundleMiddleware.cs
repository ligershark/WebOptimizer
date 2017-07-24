using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundler
{
    public class TransformMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly ITransform _transform;

        public TransformMiddleware(RequestDelegate next, IHostingEnvironment env, ITransform transform)
        {
            _next = next;
            _env = env;
            _transform = transform;
        }

        public async Task Invoke(HttpContext context)
        {
            string source = await GetContent(_transform);

            string transformedBundle = _transform.Transform(context, source);

            context.Response.ContentType = _transform.ContentType;
            await context.Response.WriteAsync(transformedBundle);
        }

        public async Task<string> GetContent(ITransform transform)
        {
            var absolutes = transform.SourceFiles.Select(f => Path.Combine(_env.WebRootPath, f));
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                sb.AppendLine(await File.ReadAllTextAsync(absolute));
            }

            return sb.ToString();
        }
    }
}

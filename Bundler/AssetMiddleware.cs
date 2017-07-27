using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bundler.Processors;
using Bundler.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace Bundler
{
    /// <summary>
    /// Middleware for setting up bundles
    /// </summary>
    public class AssetMiddleware
    {
        private readonly IAsset _asset;
        private readonly RequestDelegate _next;
        private readonly FileCache _fileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetMiddleware"/> class.
        /// </summary>
        public AssetMiddleware(RequestDelegate next, IHostingEnvironment env, IAsset asset, IMemoryCache cache)
        {
            _next = next;
            _asset = asset;
            _fileCache = new FileCache(env.WebRootFileProvider, cache);
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        private string ContentType => _asset.ContentType;

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            string cacheKey = GetCacheKey(context, _asset);

            if (IsConditionalGet(context, cacheKey))
            {
                context.Response.StatusCode = 304;
                await WriteOutputAsync(context, string.Empty, cacheKey);
            }
            else if (_fileCache.TryGetValue(cacheKey, out string value))
            {
                await WriteOutputAsync(context, value, cacheKey);
            }
            else
            {
                string result = await ExecuteAsync(context, _asset, _fileCache.FileProvider);

                if (string.IsNullOrEmpty(result))
                {
                    await _next(context);
                    return;
                }

                _fileCache.Add(cacheKey, result, _asset.SourceFiles);

                await WriteOutputAsync(context, result, cacheKey);
            }
        }

        /// <summary>
        /// Executes the bundle and returns the processed output.
        /// </summary>
        public static async Task<string> ExecuteAsync(HttpContext context, IAsset asset, IFileProvider fileProvider)
        {
            string source = await GetContentAsync(asset, fileProvider).ConfigureAwait(false);

            var config = new AssetContext(context, asset)
            {
                Content = source
            };

            foreach (IProcessor processor in asset.PostProcessors)
            {
                processor.Execute(config);
            }

            return config.Content;
        }

        private static async Task<string> GetContentAsync(IAsset asset, IFileProvider fileProvider)
        {
            IEnumerable<string> absolutes = asset.SourceFiles.Select(f => fileProvider.GetFileInfo(f).PhysicalPath);
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                sb.AppendLine(await System.IO.File.ReadAllTextAsync(absolute).ConfigureAwait(false));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        public static string GetCacheKey(HttpContext context, IAsset asset)
        {
            string cacheKey = asset.Route;

            foreach (IProcessor processors in asset.PostProcessors)
            {
                cacheKey += processors.CacheKey(context);
            }

            return cacheKey.GetHashCode().ToString();
        }

        private bool IsConditionalGet(HttpContext context, string cacheKey)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm))
            {
                return cacheKey == inm.ToString().Trim('"');
            }

            return false;
        }

        private async Task WriteOutputAsync(HttpContext context, string content, string cacheKey)
        {
            context.Response.ContentType = ContentType;

            if (!string.IsNullOrEmpty(cacheKey))
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age=31536000"; // 1 year
                context.Response.Headers["Etag"] = $"\"{cacheKey}\"";
            }

            if (!string.IsNullOrEmpty(content))
            {
                await context.Response.WriteAsync(content);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class BundleMiddleware
    {
        private readonly IBundle _bundle;
        private readonly RequestDelegate _next;
        private readonly FileCache _fileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleMiddleware"/> class.
        /// </summary>
        public BundleMiddleware(RequestDelegate next, IHostingEnvironment env, IBundle bundle, IMemoryCache cache)
        {
            _next = next;
            _bundle = bundle;
            _fileCache = new FileCache(env.WebRootFileProvider, cache);
        }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        private string ContentType => _bundle.ContentType;

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            string cacheKey = GetCacheKey(context, _bundle);

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
                string result = await ExecuteAsync(context, _bundle, _fileCache.FileProvider);

                if (string.IsNullOrEmpty(result))
                {
                    await _next(context);
                    return;
                }

                _fileCache.AddFileBundleToCache(cacheKey, result, _bundle.SourceFiles);

                await WriteOutputAsync(context, result, cacheKey);
            }
        }

        /// <summary>
        /// Executes the bundle and returns the processed output.
        /// </summary>
        public static async Task<string> ExecuteAsync(HttpContext context, IBundle bundle, IFileProvider fileProvider)
        {
            string source = await GetContentAsync(bundle, fileProvider);

            var config = new BundlerProcess(context, bundle)
            {
                Content = source
            };

            foreach (Action<BundlerProcess> processor in bundle.PostProcessors)
            {
                processor(config);
            }

            return config.Content;
        }

        private static async Task<string> GetContentAsync(IBundle bundle, IFileProvider fileProvider)
        {
            IEnumerable<string> absolutes = bundle.SourceFiles.Select(f => fileProvider.GetFileInfo(f).PhysicalPath);
            var sb = new StringBuilder();

            foreach (string absolute in absolutes)
            {
                sb.AppendLine(await File.ReadAllTextAsync(absolute));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        public static string GetCacheKey(HttpContext context, IBundle bundle)
        {
            string baseCacheKey = context.Request.PathBase + context.Request.Path;

            string transformKey = string.Join("", bundle.CacheKeys.Select(p => p.Key + p.Value));
            return (baseCacheKey + transformKey).GetHashCode().ToString();
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

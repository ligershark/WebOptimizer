using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebOptimizer
{
    /// <summary>
    /// A class for building assets.
    /// </summary>
    /// <seealso cref="WebOptimizer.IAssetBuilder" />
    public class AssetBuilder : IAssetBuilder
    {
        private IMemoryCache _cache;
        private ILogger<AssetBuilder> _logger;
        private IHostingEnvironment _env;
        private string _cacheDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBuilder"/> class.
        /// </summary>
        public AssetBuilder(IMemoryCache cache, ILogger<AssetBuilder> logger, IHostingEnvironment env)
        {
            _cache = cache;
            _logger = logger;
            _env = env;
            _cacheDir = Path.Combine(env.ContentRootPath, "obj", "WebOptimizerCache");
        }

        /// <summary>
        /// Builds an asset by running it through all the processors.
        /// </summary>
        public async Task<IAssetResponse> BuildAsync(IAsset asset, HttpContext context, IWebOptimizerOptions options)
        {
            options.EnsureDefaults(_env);
            string cacheKey = asset.GenerateCacheKey(context);

            if (_cache.TryGetValue(cacheKey, out AssetResponse value))
            {
                _logger.LogServedFromMemoryCache(context.Request.Path);
                return value;
            }
            else if (AssetResponse.TryGetFromDiskCache(context.Request.Path, cacheKey, _cacheDir, out value))
            {
                AddToCache(cacheKey, value, asset, options);
                return value;
            }
            else
            {
                byte[] bytes = await asset.ExecuteAsync(context, options).ConfigureAwait(false);

                var response = new AssetResponse(bytes, cacheKey);

                foreach (string name in context.Response.Headers.Keys)
                {
                    response.Headers.Add(name, context.Response.Headers[name]);
                }

                if (bytes == null || bytes.Length == 0)
                {
                    return null;
                }

                AddToCache(cacheKey, response, asset, options);

                await response.CacheToDiskAsync(context.Request.Path, cacheKey, _cacheDir).ConfigureAwait(false);

                _logger.LogGeneratedOutput(context.Request.Path);

                return response;
            }
        }
        private void AddToCache(string cacheKey, AssetResponse value, IAsset asset, IWebOptimizerOptions options)
        {
            if (options.EnableCaching == true)
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.SetSlidingExpiration(TimeSpan.FromHours(24));

                foreach (string file in asset.SourceFiles)
                {
                    cacheOptions.AddExpirationToken(asset.GetFileProvider(_env).Watch(file));
                }

                _cache.Set(cacheKey, value, cacheOptions);
            }
        }
    }
}

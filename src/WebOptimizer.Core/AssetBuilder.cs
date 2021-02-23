using System;
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
    internal class AssetBuilder : IAssetBuilder
    {
        private IMemoryCache _cache;
        private ILogger<AssetBuilder> _logger;
        private IWebHostEnvironment _env;
        private readonly IAssetResponseStore _assetResponseCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBuilder"/> class.
        /// </summary>
        public AssetBuilder(IMemoryCache cache, IAssetResponseStore assetResponseCache, ILogger<AssetBuilder> logger, IWebHostEnvironment env)
        {
            _cache = cache;
            _logger = logger;
            _env = env;
            _assetResponseCache = assetResponseCache;
        }

        /// <summary>
        /// Builds an asset by running it through all the processors.
        /// </summary>
        public async Task<IAssetResponse> BuildAsync(IAsset asset, HttpContext context, IWebOptimizerOptions options)
        {
            string cacheKey = asset.GenerateCacheKey(context);

            if (options.EnableMemoryCache == true && _cache.TryGetValue(cacheKey, out AssetResponse value))
            {
                _logger.LogServedFromMemoryCache(context.Request.Path);
                return value;
            }
            else if (options.EnableDiskCache == true && _assetResponseCache.TryGet(asset.Route, cacheKey, out value))
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

                if (options.AllowEmptyBundle == false && (bytes == null || bytes.Length == 0))
                {
                    return null;
                }

                AddToCache(cacheKey, response, asset, options);

                if (options.EnableDiskCache == true)
                {
                    await _assetResponseCache.AddAsync(asset.Route, cacheKey, response).ConfigureAwait(false);
                }

                _logger.LogGeneratedOutput(context.Request.Path);

                return response;
            }
        }

        private void AddToCache(string cacheKey, AssetResponse value, IAsset asset, IWebOptimizerOptions options)
        {
            if (options.EnableMemoryCache == true)
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

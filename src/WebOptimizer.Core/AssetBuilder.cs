using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebOptimizer
{
    /// <summary>
    /// A class for building assets.
    /// </summary>
    /// <seealso cref="IAssetBuilder" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="AssetBuilder"/> class.
    /// </remarks>
    internal class AssetBuilder(IMemoryCache cache, IAssetResponseStore assetResponseCache, ILogger<AssetBuilder> logger, IWebHostEnvironment env) : IAssetBuilder
    {
        /// <summary>
        /// Builds an asset by running it through all the processors.
        /// </summary>
        public async Task<IAssetResponse?> BuildAsync(IAsset asset, HttpContext context, IWebOptimizerOptions options)
        {
            string cacheKey;
            try
            {
                cacheKey = asset.GenerateCacheKey(context, options);
            }
            catch (FileNotFoundException)
            {
                logger.LogFileNotFound(context.Request.Path);
                return null;
            }

            if (options.EnableMemoryCache == true && cache.TryGetValue(cacheKey, out AssetResponse? value))
            {
                logger.LogServedFromMemoryCache(context.Request.Path);
                return value;
            }
            else if (options.EnableDiskCache == true && assetResponseCache.TryGet(asset.Route, cacheKey, out value))
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
                    response.Headers.Add(name, context.Response.Headers[name]!);
                }

                if (options.AllowEmptyBundle == false && (bytes is null || bytes.Length == 0))
                {
                    return null;
                }

                AddToCache(cacheKey, response, asset, options);

                if (options.EnableDiskCache == true)
                {
                    await assetResponseCache.AddAsync(asset.Route, cacheKey, response).ConfigureAwait(false);
                }

                logger.LogGeneratedOutput(context.Request.Path);

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
                    cacheOptions.AddExpirationToken(asset.GetFileProvider(env).Watch(file));
                }

                cache.Set(cacheKey, value, cacheOptions);
            }
        }
    }
}

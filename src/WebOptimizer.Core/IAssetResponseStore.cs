using System.Diagnostics.CodeAnalysis;

namespace WebOptimizer;

internal interface IAssetResponseStore
{
    /// <summary>
    /// Create or overwrite an entry in the cache.
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="cachekey"></param>
    /// <param name="assetResponse">An object identifying the entry.</param>
    /// <returns>
    /// <code>true</code>
    /// if the entry was created; otherwise
    /// <code>false</code>
    /// .
    /// </returns>
    Task AddAsync(string bucket, string cachekey, AssetResponse assetResponse);

    /// <summary>
    /// Remove an entry from the cache.
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="cachekey"></param>
    Task RemoveAsync(string bucket, string cachekey);

    /// <summary>
    /// Gets the <see cref="IAssetResponse"/> associated with <paramref name="cachekey"/> if present.
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="cachekey">A string identifying the requested entry.</param>
    /// <param name="assetResponse">The located value or null.</param>
    /// <returns>
    /// <code>true</code>
    /// if the key was found; otherwise
    /// <code>false</code>
    /// .
    /// </returns>
    bool TryGet(string bucket, string cachekey, [NotNullWhen(true)] out AssetResponse? assetResponse);
}

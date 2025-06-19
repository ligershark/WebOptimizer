namespace WebOptimizer;

/// <summary>
/// The response from building an asset.
/// </summary>
public interface IAssetResponse
{
    /// <summary>
    /// Gets the content of the response.
    /// </summary>
    byte[] Body { get; }

    /// <summary>
    /// Gets the cache key.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets the HTTP headers.
    /// </summary>
    Dictionary<string, string> Headers { get; }
}

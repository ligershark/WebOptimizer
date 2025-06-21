namespace WebOptimizer;

[Serializable]
internal class AssetResponse(byte[]? body = null, string? cacheKey = null) : IAssetResponse
{
    public byte[]? Body { get; set; } = body;
    public string? CacheKey { get; } = cacheKey;
    public Dictionary<string, string> Headers { get; } = [];
}

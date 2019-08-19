using System;
using System.Collections.Generic;

namespace WebOptimizer
{
    [Serializable]
    internal class AssetResponse : IAssetResponse
    {
        public AssetResponse()
        {

        }
        public AssetResponse(byte[] body, string cacheKey)
        {
            Body = body;
            CacheKey = cacheKey;
            Headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get; }

        public byte[] Body { get; set; }

        public string CacheKey { get; }
    }
}

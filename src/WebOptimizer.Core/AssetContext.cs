using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebOptimizer
{
    internal class AssetContext : IAssetContext
    {
        public AssetContext(HttpContext httpContext, IAsset asset)
        {
            Content = new Dictionary<string, byte[]>();
            HttpContext = httpContext;
            Asset = asset;
        }

        public IDictionary<string, byte[]> Content { get; set; }

        public HttpContext HttpContext { get; }

        public IAsset Asset { get; }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebOptimizer
{
    internal class AssetContext : IAssetContext
    {
        public AssetContext(HttpContext httpContext, IAsset asset, IWebOptimizerOptions options)
        {
            Content = new Dictionary<string, byte[]>();
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDictionary<string, byte[]> Content { get; set; }

        public HttpContext HttpContext { get; }

        public IAsset Asset { get; }

        public IWebOptimizerOptions Options { get; }
    }
}

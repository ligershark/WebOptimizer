using System;
using Microsoft.AspNetCore.Hosting;

namespace WebOptimizer
{
    internal class AssetMiddlewareOptions : IAssetMiddlewareOptions
    {
        public AssetMiddlewareOptions(IHostingEnvironment env)
        {
            EnableCaching = !env.IsDevelopment();
            SlidingExpiration = TimeSpan.FromHours(24);
        }

        public bool? EnableCaching { get; set; }

        public TimeSpan SlidingExpiration { get; set; }
    }
}

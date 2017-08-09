using System;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetContextTest
    {
        [Fact2]
        public void AssetContextConstructor_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };
            var httpContext = new DefaultHttpContext();

            var asset = new Asset(route, contentType, sourcefiles);
            var assetContext = new AssetContext(httpContext, asset);

            Assert.Equal(asset, assetContext.Asset);
            Assert.Equal(httpContext, assetContext.HttpContext);
            Assert.Equal(0, assetContext.Content.Count);
        }

        [Fact2]
        public void AssetContextConstructor_NullAsset()
        {
            var httpContext = new DefaultHttpContext();

            Assert.Throws<ArgumentNullException>(() => new AssetContext(httpContext, null));
        }

        [Fact2]
        public void AssetContextConstructor_NullHttpContext()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };
            var httpContext = new DefaultHttpContext();

            var asset = new Asset(route, contentType, sourcefiles);

            Assert.Throws<ArgumentNullException>(() => new AssetContext(null, asset));
        }
    }
}

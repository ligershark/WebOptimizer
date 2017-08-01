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

            var asset = Asset.Create(route, contentType, sourcefiles);
            var assetContext = new AssetContext(httpContext, asset);

            Assert.Equal(asset, assetContext.Asset);
            Assert.Equal(httpContext, assetContext.HttpContext);
            Assert.Equal(string.Empty, assetContext.Content);
        }
    }
}

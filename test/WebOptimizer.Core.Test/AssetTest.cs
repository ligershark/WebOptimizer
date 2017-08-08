using Microsoft.AspNetCore.Http;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetTest
    {
        [Fact2]
        public void AssetCreate_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };

            var asset = Asset.Create(route, contentType, sourcefiles);

            Assert.Equal(route, asset.Route);
            Assert.Equal(contentType, asset.ContentType);
            Assert.Equal(sourcefiles, asset.SourceFiles);
            Assert.Equal(0, asset.Processors.Count);
        }

        [Fact2]
        public void GenerateCacheKey_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };
            var context = new DefaultHttpContext();

            var asset = Asset.Create(route, contentType, sourcefiles);


            // Check non-gzip value
            string key = asset.GenerateCacheKey(context);
            Assert.Equal("_BZuuBNh_zEXnNPIPaO_4Ii4UdM", key);

            // Check gzip value
            context.Request.Headers["Accept-Encoding"] = "gzip, deflate";
            string gzipKey = asset.GenerateCacheKey(context);
            Assert.Equal("SvH6WGVAapgMXiPenaOGnKS_oMI", gzipKey);
        }
    }
}

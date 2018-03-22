using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace WebOptimizer.Core.Test
{
    public class AssetResponseStoreTest
    {
        [Fact2]
        public async Task RoundTrip_Success()
        {
            var logger = new Mock<ILogger<AssetResponseStore>>();
            var env = new Mock<IHostingEnvironment>().SetupAllProperties();
            env.Setup(e => e.ContentRootPath).Returns(Environment.CurrentDirectory);
            string path = Path.Combine(Environment.CurrentDirectory, "WebOptimizerTest");
            string filePath = Path.Combine(path, "bucket__cachekey.cache");

            var woo = new Mock<IConfigureOptions<WebOptimizerOptions>>();
            woo.Setup(o => o.Configure(It.IsAny<WebOptimizerOptions>()))
                .Callback<WebOptimizerOptions>(o => o.CacheDirectory = path);

            byte[] body = "*{color:red}".AsByteArray();
            var before = new AssetResponse(body, "cachekey");

            IAssetResponseStore ars = new AssetResponseStore(logger.Object, env.Object, woo.Object);

            await ars.AddAsync("bucket", "cachekey", before).ConfigureAwait(false);
            Assert.True(File.Exists(filePath));

            Assert.True(ars.TryGet("bucket", "cachekey", out var after));
            Assert.Equal(before.CacheKey, after.CacheKey);
            Assert.Equal(before.Body, after.Body);

            await ars.RemoveAsync("bucket", "cachekey").ConfigureAwait(false);

            Assert.False(File.Exists(filePath));
        }
    }
}

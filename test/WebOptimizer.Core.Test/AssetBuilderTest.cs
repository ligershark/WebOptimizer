using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace WebOptimizer.Core.Test
{
    public class AssetBuilderTest
    {
        [Fact2]
        public async Task AssetBuilder_NoMemoryCache()
        {
            byte[] cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableMemoryCache = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>())).Returns("cachekey");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            response.Setup(r => r.Headers.Keys).Returns(new string[] { });
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values)).Returns(false);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Request.Path).Returns("/file.css");

            var next = new Mock<RequestDelegate>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            env.Setup(e => e.ContentRootPath).Returns(@"D:\Project\");

            object bytes;
            var cache = new Mock<IMemoryCache>();
            cache.Setup(c => c.TryGetValue(It.IsAny<string>(), out bytes)).Throws<InvalidOperationException>();

            var store = new Mock<IAssetResponseStore>();

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetBuilder>>();
            var builder = new AssetBuilder(cache.Object, store.Object, logger.Object, env.Object);

            var result = await builder.BuildAsync(asset.Object, context.Object, options).ConfigureAwait(false);

            Assert.Equal(cssContent, result.Body);
        }

        [Fact2]
        public async Task AssetBuilder_NoDiskCache()
        {
            byte[] cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableDiskCache = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>())).Returns("cachekey");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            response.Setup(r => r.Headers.Keys).Returns(new string[] { });
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values)).Returns(false);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Request.Path).Returns("/file.css");

            var next = new Mock<RequestDelegate>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            env.Setup(e => e.ContentRootPath).Returns(@"D:\Project\");

            var mco = new Mock<IOptions<MemoryCacheOptions>>();
            mco.SetupGet(o => o.Value).Returns(new MemoryCacheOptions());
            var cache = new MemoryCache(mco.Object);

            AssetResponse ar;
            var store = new Mock<IAssetResponseStore>();
            store.Setup(s => s.TryGet(It.IsAny<string>(), It.IsAny<string>(), out ar)).Throws<InvalidOperationException>();

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var logger = new Mock<ILogger<AssetBuilder>>();
            var builder = new AssetBuilder(cache, store.Object, logger.Object, env.Object);

            var result = await builder.BuildAsync(asset.Object, context.Object, options).ConfigureAwait(false);

            Assert.Equal(cssContent, result.Body);
        }

        [Fact2]
        public async Task AssetBuilder_NoMemoryCache_and_NoDiskCache()
        {
            byte[] cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableMemoryCache = false, EnableDiskCache = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>())).Returns("cachekey");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            response.Setup(r => r.Headers.Keys).Returns(new string[] { });
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values)).Returns(false);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Request.Path).Returns("/file.css");

            var next = new Mock<RequestDelegate>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            env.Setup(e => e.ContentRootPath).Returns(@"D:\Project\");

            object bytes;
            var cache = new Mock<IMemoryCache>();
            cache.Setup(c => c.TryGetValue(It.IsAny<string>(), out bytes)).Throws<InvalidOperationException>();
            AssetResponse ar;
            var store = new Mock<IAssetResponseStore>();
            store.Setup(s => s.TryGet(It.IsAny<string>(), It.IsAny<string>(), out ar)).Throws<InvalidOperationException>();

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var logger = new Mock<ILogger<AssetBuilder>>();
            var builder = new AssetBuilder(cache.Object, store.Object, logger.Object, env.Object);

            var result = await builder.BuildAsync(asset.Object, context.Object, options).ConfigureAwait(false);

            Assert.Equal(cssContent, result.Body);
        }
    }
}

using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetMiddlewareTest
    {
        [Fact2]
        public async Task AssetMiddleware_NoCache()
        {
            string cssContent = "*{color:red}";

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableCaching = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent.AsByteArray()));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values))
                   .Returns(false);
            context.Setup(c => c.Response)
                   .Returns(response.Object);

            context.Setup(c => c.Request.Path).Returns("/file.css");
            context.Setup(c => c.Features.Get<IHttpsCompressionFeature>()).Returns((IHttpsCompressionFeature) null);

            response.SetupGet(c => c.Headers)
                   .Returns(new HeaderDictionary());

            var next = new Mock<RequestDelegate>();
            var env = new HostingEnvironment();
            var cache = new Mock<IMemoryCache>();

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var mcr = new AssetResponse(cssContent.AsByteArray(), null);
            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            builder.Setup(b => b.BuildAsync(It.IsAny<IAsset>(), context.Object, options)).Returns(Task.FromResult<IAssetResponse>(mcr));
            var middleware = new AssetMiddleware(next.Object, pipeline, logger.Object, builder.Object);
            var stream = new MemoryStream();

            response.Setup(r => r.Body).Returns(stream);
            await middleware.InvokeAsync(context.Object, amo.Object);

            Assert.Equal("text/css", context.Object.Response.ContentType);
            Assert.Equal(cssContent.AsByteArray(), await stream.AsBytesAsync());
            Assert.Equal(0, response.Object.StatusCode);
        }

        [Fact2]
        public async Task AssetMiddleware_NoCache_EmptyResponse()
        {
            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableCaching = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(new byte[0]));

            StringValues values;
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values))
                   .Returns(false);

            context.Setup(c => c.Request.Path).Returns("/file.css");

            context.SetupGet(c => c.Response.Headers)
                   .Returns(new HeaderDictionary());

            var next = new Mock<RequestDelegate>();
            var env = new HostingEnvironment();
            var cache = new Mock<IMemoryCache>();

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            var middleware = new AssetMiddleware(next.Object, pipeline, logger.Object, builder.Object);

            var stream = new MemoryStream();

            await middleware.InvokeAsync(context.Object, amo.Object);

            next.Verify(n => n(context.Object), Times.Once);
        }

        [Fact2]
        public async Task AssetMiddleware_Cache()
        {
            var cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableCaching = false };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values))
                   .Returns(false);
            context.Setup(c => c.Response)
                   .Returns(response.Object);

            context.Setup(c => c.Request.Path).Returns("/file.css");
            context.Setup(c => c.Features.Get<IHttpsCompressionFeature>()).Returns((IHttpsCompressionFeature) null);

            var next = new Mock<RequestDelegate>();
            var env = new HostingEnvironment();
            var cache = new Mock<IMemoryCache>();
            var mcr = new AssetResponse(cssContent, null);

            object bytes = mcr;
            cache.Setup(c => c.TryGetValue(It.IsAny<string>(), out bytes))
                 .Returns(true);

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            builder.Setup(b => b.BuildAsync(It.IsAny<IAsset>(), context.Object, options)).Returns(Task.FromResult<IAssetResponse>(mcr));
            var middleware = new AssetMiddleware(next.Object, pipeline, logger.Object, builder.Object);
            var stream = new MemoryStream();

            response.Setup(r => r.Body).Returns(stream);
            await middleware.InvokeAsync(context.Object, amo.Object);

            Assert.Equal("text/css", context.Object.Response.ContentType);
            Assert.Equal(cssContent, await stream.AsBytesAsync());
            Assert.Equal(0, response.Object.StatusCode);
        }

        [Fact2]
        public async Task AssetMiddleware_Conditional()
        {
            var cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { EnableCaching = true };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>())).Returns("etag");

            StringValues values = "etag";
            var response = new Mock<HttpResponse>().SetupAllProperties();
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("If-None-Match", out values))
                   .Returns(true);
            context.Setup(c => c.Response)
                   .Returns(response.Object);
            context.Setup(c => c.Request.Query.ContainsKey(It.IsAny<string>()))
                   .Returns(true);

            context.Setup(c => c.Request.Path).Returns("/file.css");

            response.SetupGet(c => c.Headers)
                   .Returns(new HeaderDictionary());

            var next = new Mock<RequestDelegate>();
            var env = new HostingEnvironment();
            var cache = new Mock<IMemoryCache>();
            var mcr = new AssetResponse(cssContent, values);

            object bytes = mcr;
            cache.Setup(c => c.TryGetValue(It.IsAny<string>(), out bytes))
                 .Returns(true);

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            builder.Setup(b => b.BuildAsync(It.IsAny<IAsset>(), context.Object, options)).Returns(Task.FromResult<IAssetResponse>(mcr));
            var middleware = new AssetMiddleware(next.Object, pipeline, logger.Object, builder.Object);
            var stream = new MemoryStream();

            response.Setup(r => r.Body).Returns(stream);
            await middleware.InvokeAsync(context.Object, amo.Object);

            Assert.Equal("text/css", context.Object.Response.ContentType);
            Assert.Equal(0, stream.Length);
            Assert.Equal(304, response.Object.StatusCode);
        }

        [Fact2]
        public async Task AssetMiddleware_NoAssetMatch()
        {
            IAsset asset;
            var pipeline = new Mock<IAssetPipeline>();
            pipeline.Setup(p => p.TryGetAssetFromRoute(It.IsAny<string>(), out asset))
                    .Returns(false);

            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request.Path).Returns("/file.css");

            var next = new Mock<RequestDelegate>();
            var env = new HostingEnvironment();
            var cache = new Mock<IMemoryCache>();

            var options = new WebOptimizerOptions() { EnableCaching = false };
            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            var middleware = new AssetMiddleware(next.Object, pipeline.Object, logger.Object, builder.Object);

            await middleware.InvokeAsync(context.Object, amo.Object);

            next.Verify(n => n(context.Object), Times.Once);

        }

        [Fact2]
        public async Task AssetMiddleware_Compression()
        {
            var cssContent = "*{color:red}".AsByteArray();

            var pipeline = new AssetPipeline();
            var options = new WebOptimizerOptions() { HttpsCompression = HttpsCompressionMode.Compress };
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("/file.css");
            asset.Setup(a => a.ExecuteAsync(It.IsAny<HttpContext>(), options))
                 .Returns(Task.FromResult(cssContent));

            StringValues values;
            var response = new Mock<HttpResponse>().SetupAllProperties();
            var context = new Mock<HttpContext>().SetupAllProperties();
            context.Setup(s => s.Request.Headers.TryGetValue("Accept-Encoding", out values))
                   .Returns(false);
            context.Setup(c => c.Response)
                   .Returns(response.Object);

            context.Setup(c => c.Request.Path).Returns("/file.css");

            var compressionFeature = new HttpsCompressionFeature();
            context.Setup(c => c.Features.Get<IHttpsCompressionFeature>()).Returns(compressionFeature);

            var next = new Mock<RequestDelegate>();
            var cache = new Mock<IMemoryCache>();
            var mcr = new AssetResponse(cssContent, null);

            object bytes = mcr;
            cache.Setup(c => c.TryGetValue(It.IsAny<string>(), out bytes))
                 .Returns(true);

            pipeline._assets = new ConcurrentDictionary<string, IAsset>();
            pipeline._assets.TryAdd(asset.Object.Route, asset.Object);

            var amo = new Mock<IOptionsSnapshot<WebOptimizerOptions>>();
            amo.SetupGet(a => a.Value).Returns(options);

            var logger = new Mock<ILogger<AssetMiddleware>>();
            var builder = new Mock<IAssetBuilder>();
            builder.Setup(b => b.BuildAsync(It.IsAny<IAsset>(), context.Object, options)).Returns(Task.FromResult<IAssetResponse>(mcr));
            var middleware = new AssetMiddleware(next.Object, pipeline, logger.Object, builder.Object);
            var stream = new MemoryStream();

            response.Setup(r => r.Body).Returns(stream);
            await middleware.InvokeAsync(context.Object, amo.Object);

            Assert.Equal("text/css", context.Object.Response.ContentType);
            Assert.Equal(cssContent, await stream.AsBytesAsync());
            Assert.Equal(0, response.Object.StatusCode);

            Assert.Equal(HttpsCompressionMode.Compress, compressionFeature.Mode);
        }

        private class HttpsCompressionFeature : IHttpsCompressionFeature
        {
            public HttpsCompressionMode Mode { get; set; } = HttpsCompressionMode.Default;
        }
    }
}

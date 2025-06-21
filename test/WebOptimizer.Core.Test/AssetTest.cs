using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
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
            var logger = new Mock<ILogger<Asset>>();

            var asset = new Asset(route, contentType, sourcefiles, logger.Object);

            Assert.Equal(route, asset.Route);
            Assert.Equal(contentType, asset.ContentType);
            Assert.Equal(sourcefiles, asset.SourceFiles);
            Assert.Empty(asset.Processors);
        }

        [Fact2]
        public void AssetCreateMultipleSourceFiles_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css", "file2.css" };
            var logger = new Mock<ILogger<Asset>>();

            var asset = new Asset(route, contentType, sourcefiles, logger.Object);

            Assert.Equal(route, asset.Route);
            Assert.Equal(contentType, asset.ContentType);
            Assert.Equal(sourcefiles, asset.SourceFiles);
            Assert.Empty(asset.Processors);
        }


        [Fact2]
        public void GenerateCacheKey_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };
            var logger = new Mock<ILogger<Asset>>();

            var context = new Mock<HttpContext>().SetupAllProperties();
            var options = new WebOptimizerOptions() { EnableCaching = true };
            var env = new Mock<IWebHostEnvironment>();
            var cache = new Mock<IMemoryCache>();
            var fileProvider = new PhysicalFileProvider(Path.GetTempPath());

            var asset = new Asset(route, contentType, sourcefiles, logger.Object);
            asset.Items.Add("PhysicalFiles", Array.Empty<string>());

            StringValues ae = "gzip, deflate";
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                   .Returns(false)
                   .Returns(true);

            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                   .Returns(env.Object);

            context.Setup(c => c.RequestServices.GetService(typeof(IMemoryCache)))
                   .Returns(cache.Object);

            env.Setup(e => e.WebRootFileProvider)
                .Returns(fileProvider);

            // Check non-gzip value
            string key = asset.GenerateCacheKey(context.Object, options);
            Assert.Equal("ioTkBsCKyVlPRyIkBlmPdZjhXFX4BEsEgT4oyd7nCXY", key);

            // Check gzip value
            string gzipKey = asset.GenerateCacheKey(context.Object, options);
            Assert.Equal("ohBCDBt7xsmLL5fnMs_sJeEY5tlO7b6VvXmOWU002G8", gzipKey);
        }

        [Fact2]
        public void AssetToString()
        {
            var logger = new Mock<ILogger<Asset>>();
            var asset = new Asset("/route", "content/type", [], logger.Object);

            Assert.Equal(asset.Route, asset.ToString());
        }
    }
}

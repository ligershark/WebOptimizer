using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using WebOptimizer.Core.Test.Mocks;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class CssFinterprinterTest
    {
        [Theory2]
        [InlineData("url(/css/img/foo.png)", "url(/css/img/foo.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI)")]
        [InlineData("url(/css/img/foo.png?1=1)", "url(/css/img/foo.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI&1=1)")]
        [InlineData("url('/css/img/foo.png')", "url('/css/img/foo.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI')")]
        [InlineData("url('/img/doesntexist.png')", "url('/img/doesntexist.png')")]
        [InlineData("url(http://example.com/foo.png)", "url(http://example.com/foo.png)")]
        [InlineData("url(//example.com/foo.png)", "url(//example.com/foo.png)")]
        [InlineData("url(img/foo.png)", "url(img/foo.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI)")]
        [InlineData("url(../img/foo2.png)", "url(../img/foo2.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI)")]
        [InlineData("url(../img/foo2.png?1=1)", "url(../img/foo2.png?v=Ai9EHcgOXDloih8M5cRTS07P-FI&1=1)")]
        [InlineData("url(../img/doesntexist.png)", "url(../img/doesntexist.png)")]
        [InlineData("url(../../img/doesntexist.png)", "url(../../img/doesntexist.png)")]
        public async Task CssFingerprint_Success(string url, string newUrl)
        {
            var adjuster = new CssFingerprinter();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            context.SetupGet(s => s.Asset.Route)
                .Returns(AssetPipeline.NormalizeRoute("~/css/site.css"));

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IAssetPipeline)))
                .Returns(pipeline.Object);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);

            context.Setup(s => s.HttpContext.Request.PathBase)
                .Returns("/parent");

            env.SetupGet(e => e.WebRootFileProvider)
                .Returns(fileProvider.Object);

            env.SetupGet(e => e.WebRootPath)
                .Returns("/wwwroot");

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                .Returns((string path) => new NotFoundFileInfo(UrlPathUtils.GetFileName(path)));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/img/foo.png", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("foo.png", new DateTime(2017, 1, 1), []));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/img/foo2.png", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("foo2.png", new DateTime(2017, 1, 1), []));

            context.Object.Content = new Dictionary<string, byte[]> { { "css/site.css", url.AsByteArray() } };

            await adjuster.ExecuteAsync(context.Object);
            string result = context.Object.Content.First().Value.AsString();

            Assert.Equal(newUrl, result);
            Assert.Equal("", adjuster.CacheKey(new DefaultHttpContext(), context.Object));
        }
    }
}

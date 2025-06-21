using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using WebOptimizer.Core.Test.Mocks;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Core.Test.Processors;

public class CssFinterprinterTest
{
    [Theory2]
    [InlineData("url(/css/img/foo.png)", "url(/css/img/foo.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus)")]
    [InlineData("url(/css/img/foo.png?1=1)", "url(/css/img/foo.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus&1=1)")]
    [InlineData("url('/css/img/foo.png')", "url('/css/img/foo.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus')")]
    [InlineData("url('/img/doesntexist.png')", "url('/img/doesntexist.png')")]
    [InlineData("url(http://example.com/foo.png)", "url(http://example.com/foo.png)")]
    [InlineData("url(//example.com/foo.png)", "url(//example.com/foo.png)")]
    [InlineData("url(img/foo.png)", "url(img/foo.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus)")]
    [InlineData("url(../img/foo2.png)", "url(../img/foo2.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus)")]
    [InlineData("url(../img/foo2.png?1=1)", "url(../img/foo2.png?v=UOMNppUf6ogNVsHJDWz1gBbnvcYv5db8bif3J4YbRus&1=1)")]
    [InlineData("url(../img/doesntexist.png)", "url(../img/doesntexist.png)")]
    [InlineData("url(../../img/doesntexist.png)", "url(../../img/doesntexist.png)")]
    public async Task CssFingerprint_Success(string url, string newUrl)
    {
        var context = new Mock<IAssetContext>().SetupAllProperties();
        var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
        var asset = new Mock<IAsset>().SetupAllProperties();
        var env = new Mock<IWebHostEnvironment>();
        var fileProvider = new Mock<IFileProvider>();

        _ = context.SetupGet(s => s.Asset.Route)
            .Returns(AssetPipeline.NormalizeRoute("~/css/site.css"));

        _ = context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IAssetPipeline)))
            .Returns(pipeline.Object);

        _ = context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
            .Returns(env.Object);

        _ = context.Setup(s => s.HttpContext.Request.PathBase)
            .Returns("/parent");

        _ = env.SetupGet(e => e.WebRootFileProvider)
            .Returns(fileProvider.Object);

        _ = env.SetupGet(e => e.WebRootPath)
            .Returns("/wwwroot");

        _ = fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
            .Returns((string path) => new NotFoundFileInfo(UrlPathUtils.GetFileName(path)!));

        _ = fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/img/foo.png", StringComparison.InvariantCultureIgnoreCase))))
            .Returns(new MockFileInfo("foo.png", new DateTime(2017, 1, 1), []));

        _ = fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/img/foo2.png", StringComparison.InvariantCultureIgnoreCase))))
            .Returns(new MockFileInfo("foo2.png", new DateTime(2017, 1, 1), []));

        context.Object.Content = new Dictionary<string, byte[]> { { "css/site.css", url.AsByteArray() } };

        var adjuster = new CssFingerprinter();
        await adjuster.ExecuteAsync(context.Object);
        string result = context.Object.Content.First().Value.AsString();

        Assert.Equal(newUrl, result);
        Assert.Equal("", adjuster.CacheKey(new DefaultHttpContext(), context.Object));
    }
}

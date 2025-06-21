using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Moq;
using WebOptimizer.Core.Test.Mocks;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Core.Test.Processors;

public class CssImageInlinerTest
{
    [Theory2]
    [InlineData("url(/css/img/test.png)", "url(data:image/png;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.png?1=1)", "url(data:image/png;base64,ZW1wdHk=)")]
    [InlineData("url('/css/img/test.png')", "url('data:image/png;base64,ZW1wdHk=')")]
    [InlineData("url('/img/doesntexist.png')", "url('/img/doesntexist.png')")]
    [InlineData("url(http://test.png)", "url(http://test.png)")]
    [InlineData("url(data:image/png;base64,ZW1w)", "url(data:image/png;base64,ZW1w)")]
    [InlineData("url(img/test.png)", "url(data:image/png;base64,ZW1wdHk=)")]
    [InlineData("url(../css/img/test.png)", "url(data:image/png;base64,ZW1wdHk=)")]
    [InlineData("url('img/doesntexist.png')", "url('img/doesntexist.png')")]
    [InlineData("url(../../css/img/test.png)", "url(../../css/img/test.png)")]
    [InlineData("url(/css/img/bigfile.png)", "url(/css/img/bigfile.png)")]
    [InlineData("url(/css/img/test.jpg)", "url(data:image/jpeg;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.jpeg)", "url(data:image/jpeg;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.gif)", "url(data:image/gif;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.webp)", "url(data:image/webp;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.svg)", "url(data:image/svg+xml;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.ttf)", "url(data:application/x-font-ttf;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.otf)", "url(data:application/x-font-opentype;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.woff)", "url(data:application/font-woff;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.woff2)", "url(data:application/font-woff2;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.eot)", "url(data:application/vnd.ms-fontobject;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.sfnt)", "url(data:application/font-sfnt;base64,ZW1wdHk=)")]
    [InlineData("url(/css/img/test.unknown)", "url(/css/img/test.unknown)")]
    public async Task CssImageInliner_Success(string url, string newUrl)
    {
        var context = new Mock<IAssetContext>().SetupAllProperties();
        var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
        var asset = new Mock<IAsset>().SetupAllProperties();
        var env = new Mock<IWebHostEnvironment>();
        var fileProvider = new Mock<IFileProvider>();

        _ = context.SetupGet(s => s.Asset.Route)
            .Returns("/css/all.css");

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

        _ = fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value =>
                value.StartsWith("/css/img/test.", StringComparison.InvariantCultureIgnoreCase))))
            .Returns((string path) => new MockFileInfo(UrlPathUtils.GetFileName(path)!, new DateTime(2017, 1, 1), "empty".AsByteArray()));

        _ = fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value =>
                value.Equals("/css/img/bigfile.png", StringComparison.InvariantCultureIgnoreCase))))
            .Returns(new MockFileInfo("bigfile.png", new DateTime(2017, 1, 1), [.. Enumerable.Repeat<byte>(1, 1000)]));

        context.Object.Content = new Dictionary<string, byte[]> { { "css/site.css", url.AsByteArray() } };

        var processor = new CssImageInliner(100);
        await processor.ExecuteAsync(context.Object);
        string result = context.Object.Content.First().Value.AsString();

        Assert.Equal(newUrl, result);
    }
}

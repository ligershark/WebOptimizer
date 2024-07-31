using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class RelativePathAdjusterTest
    {
        [Theory2]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(/img/foo.png)", "url(/img/foo.png)")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(/img/foo.png?1=1)", "url(/img/foo.png?1=1)")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(img/foo.png)", "url(../css/img/foo.png)")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(http://foo.png)", "url(http://foo.png)")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url('img/foo.png')", "url('../css/img/foo.png')")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(\"img/foo.png\")", "url(\"../css/img/foo.png\")")]
        [InlineData(null, "/dist/all.css", "css/sub/site.css", "url(img/foo.png)", "url(../css/sub/img/foo.png)")]
        [InlineData(null, "/css/all.css", "css/site.css", "url(img/foo.png)", "url(img/foo.png)")]
        [InlineData(null, "/css/all.css", "css/site.css", "url(../img/foo.png)", "url(../img/foo.png)")]
        [InlineData(null, "/css/all.css", "css/site.css", "url(../../img/foo.png)", "url(../../img/foo.png)")]
        [InlineData(null, "/dist/sub/all.css", "css/sub/site.css", "url(img/foo.png)", "url(../../css/sub/img/foo.png)")]
        [InlineData(null, "/dist/all.css", "css/site.css", "url(../img/foo.png)", "url(../img/foo.png)")]
        [InlineData(null, "dist/all.css", "css/site.css", "url(img/foo.png)", "url(../css/img/foo.png)")]
        [InlineData(null, "dist/all.css", "css/site.css", "url(../img/foo.png)", "url(../img/foo.png)")]
        [InlineData("/parent", "/css/all.css", "css/site.css", "url(../img/foo.png)", "url(../img/foo.png)")]
        [InlineData("/parent", "/css/all.css", "css/site.css", "url(img/foo.png)", "url(img/foo.png)")]
        [InlineData("/parent", "/dist/all.css", "css/site.css", "url(/img/foo.png)", "url(/img/foo.png)")]
        public async Task AdjustRelativePaths_Success(string? requestPathBase, string route, string inputPath, string url, string newUrl)
        {
            var adjuster = new RelativePathAdjuster();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            env.SetupGet(s => s.WebRootPath)
                .Returns(@"//source");

            context.SetupGet(s => s.Asset.Route)
                   .Returns(route);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IAssetPipeline)))
                   .Returns(pipeline.Object);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                   .Returns(env.Object);

            if (requestPathBase != null)
            {
                context.Setup(s => s.HttpContext.Request.PathBase)
                    .Returns(requestPathBase);
            }

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                .Returns((string path) => new NotFoundFileInfo(UrlPathUtils.GetFileName(path)));

            context.Object.Content = new Dictionary<string, byte[]> { { inputPath, url.AsByteArray() } };

            await adjuster.ExecuteAsync(context.Object);

            Assert.Equal(newUrl, context.Object.Content.First().Value.AsString());
            Assert.Equal("", adjuster.CacheKey(new DefaultHttpContext(), context.Object));
        }
    }
}

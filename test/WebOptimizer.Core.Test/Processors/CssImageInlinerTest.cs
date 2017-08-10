using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical;
using Moq;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class CssImageInlinerTest
    {
        [Theory2]
        [InlineData("url(/img/test.png)", "url('data:image/png;base64,ZW1wdHk=')")]
        [InlineData("url(/img/test.png?1=1)", "url('data:image/png;base64,ZW1wdHk=')")]
        [InlineData("url('/img/test.png')", "url('data:image/png;base64,ZW1wdHk=')")]
        [InlineData("url('/img/doesntexist.png')", "url('/img/doesntexist.png')")]
        [InlineData("url(http://test.png)", "url(http://test.png)")]
        public async Task CssImageInliner_Success(string url, string newUrl)
        {
            var processor = new CssImageInliner(5120);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();

            string temp = Path.GetTempPath();
            string path = Path.Combine(temp, "css", "img");
            Directory.CreateDirectory(path);
            string imagePath = Path.Combine(path, "test.png");
            File.WriteAllText(imagePath, "empty");
            File.SetLastWriteTime(imagePath, new DateTime(2017, 1, 1));

            var inputFile = new PhysicalFileInfo(new FileInfo(Path.Combine(temp, "css", "site.css")));
            var outputFile = new PhysicalFileInfo(new FileInfo(Path.Combine(temp, "dist", "all.css")));

            context.SetupGet(s => s.Asset.Route)
                   .Returns("/my/route.css");

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IAssetPipeline)))
                   .Returns(pipeline.Object);

            var options = new Mock<WebOptimizerOptions>();
            options.Setup(s => s.FileProvider.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile);

            context.SetupGet(s => s.Options)
                   .Returns(options.Object);

            context.Object.Content = new Dictionary<string, byte[]> { { "css/site.css", url.AsByteArray() } };


            await processor.ExecuteAsync(context.Object);
            string result = context.Object.Content.First().Value.AsString();

            Assert.Equal(newUrl, result);
        }
    }
}

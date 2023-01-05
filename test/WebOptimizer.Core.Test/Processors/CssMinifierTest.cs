using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUglify.Css;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class CssMinifierTest
    {
        [Fact2]
        public async Task MinifyCss_DefaultSettings_Success()
        {
            var minifier = new CssMinifier(new CssSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", "body { color: yellow; }".AsByteArray() } };
            var options = new Mock<WebOptimizerOptions>();

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("body{color:#ff0}", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
        }

        [Theory2]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("/* comment */")]
        [InlineData("   /**/ ")]
        [InlineData("\r\n  \t \r \n")]
        public async Task MinifyCss_EmptyContent_Success(string input)
        {
            var minifier = new CssMinifier(new CssSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", input.AsByteArray() } };
            var options = new Mock<WebOptimizerOptions>();

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
        }

        [Fact2]
        public async Task MinifyCss_CustomSettings_Success()
        {
            var settings = new CssSettings { TermSemicolons = true, ColorNames = CssColor.NoSwap };
            var minifier = new CssMinifier(settings);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", "body { color: yellow; }".AsByteArray() } };
            var options = new Mock<WebOptimizerOptions>();

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("body{color:yellow;}", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
        }

        [Fact2]
        public async Task MinifyCss_NullSettings_Success()
        {
            var minifier = new CssMinifier(null);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", "body { color: yellow; }".AsByteArray() } };
            var options = new Mock<WebOptimizerOptions>();

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("body{color:#ff0}", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
        }

        [Fact2]
        public void AddCssBundle_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCssBundle("/foo.css", "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count);
            Assert.Equal(6, asset.Processors.Count);
        }

        [Fact2]
        public void AddCssBundle_DefaultSettings_SuccessRelative()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCssBundle("foo.css", "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count);
            Assert.Equal(6, asset.Processors.Count);
        }

        [Fact2]
        public void AddCssBundle_CustomSettings_Success()
        {
            var settings = new CssSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCssBundle("/foo.css", settings, "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count);
            Assert.Equal(6, asset.Processors.Count);
        }

        [Fact2]
        public void AddCssFiles_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.MinifyCssFiles().First();

            Assert.Equal("/**/*.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.True(1 == asset.SourceFiles.Count);
            Assert.True(3 == asset.Processors.Count);
        }
    }
}

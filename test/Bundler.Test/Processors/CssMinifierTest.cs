using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUglify.Css;
using Xunit;

namespace Bundler.Test.Processors
{
    public class CssMinifierTest
    {
        [Fact2]
        public async Task ExecuteTest_DefaultSettings_Success()
        {
            var minifier = new CssMinifier(new CssSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "body { color: yellow; }";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("body{color:#ff0}", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Theory2]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("/* comment */")]
        [InlineData("   /**/ ")]
        [InlineData("\r\n  \t \r \n")]
        public async Task ExecuteTest_EmptyContent_Success(string input)
        {
            var minifier = new CssMinifier(new CssSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = input;

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task ExecuteTest_CustomSettings_Success()
        {
            var settings = new CssSettings { TermSemicolons = true, ColorNames = CssColor.NoSwap };
            var minifier = new CssMinifier(settings);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "body { color: yellow; }";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("body{color:yellow;}", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }
    }
}

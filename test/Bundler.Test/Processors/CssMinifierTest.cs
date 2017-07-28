using System;
using Bundler.Processors;
using NUglify.Css;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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

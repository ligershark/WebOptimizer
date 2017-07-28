using System.Threading.Tasks;
using Bundler.Processors;
using Microsoft.AspNetCore.Http;
using Moq;
using NUglify.JavaScript;
using Xunit;

namespace Bundler.Test.Processors
{
    public class JavaScriptMinifierTest
    {
        [Fact2]
        public async Task ExecuteTest_DefaultSettings_Success()
        {
            var minifier = new JavaScriptMinifier(new CodeSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "var i = 0;";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("var i=0", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task ExecuteTest_EmptyContent_Success()
        {
            var minifier = new JavaScriptMinifier(new CodeSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task ExecuteTest_WhitespaceContent_Success()
        {
            var minifier = new JavaScriptMinifier(new CodeSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "   ";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task ExecuteTest_CommentContent_Success()
        {
            var minifier = new JavaScriptMinifier(new CodeSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "// Some comment";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task ExecuteTest_CustomSettings_Success()
        {
            var settings = new CodeSettings { TermSemicolons = true};
            var minifier = new JavaScriptMinifier(settings);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = "var i = 0;";

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("var i=0;", context.Object.Content);
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }
    }
}

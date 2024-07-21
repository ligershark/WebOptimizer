using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class EnforceFileExtensionsTest
    {
        [Theory2]
        [InlineData("fo.hat", ".js")]
        [InlineData("fo.hat", ".js", ".jsx")]
        [InlineData(".js", ".jsx")]
        [InlineData("/foo/bar.js/fo.hat", ".js", ".jsx")]
        public async Task EnforceExtension_Throws(string fileName, params string[] extensions)
        {
            var minifier = new EnforceFileExtensions(extensions);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { fileName, new byte[0] } };
            var options = new Mock<WebOptimizerOptions>();

            await Assert.ThrowsAsync<NotSupportedException>(async () => await minifier.ExecuteAsync(context.Object));
        }

        [Theory2]
        [InlineData("fo.js", ".js")]
        [InlineData("fo.jsx", ".js", ".jsx")]
        [InlineData(".js", ".js")]
        [InlineData("/foo/bar/fo.jsx", ".js", ".jsx")]
        public async Task EnforceExtension_Success(string fileName, params string[] extensions)
        {
            var minifier = new EnforceFileExtensions(extensions);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { fileName, new byte[0] } };
            var options = new Mock<WebOptimizerOptions>();

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal(fileName, context.Object.Content.First().Key);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class UseContentRootTest
    {
        [Fact2]
        public void UseContentRoot_Success()
        {
            var minifier = new UseContentRoot();
            var logger = new Mock<ILogger<Asset>>();
            var asset = new Asset("", "", [""], logger.Object);

            Assert.Empty(asset.Items);
            asset.UseContentRoot();
            Assert.Single(asset.Items);
        }

        [Fact2]
        public void UseFileProvider_Success()
        {
            var minifier = new UseContentRoot();
            var logger = new Mock<ILogger<Asset>>();
            var asset = new Asset("", "", [""], logger.Object);

            Assert.Empty(asset.Items);
            asset.UseFileProvider(null);
            Assert.Single(asset.Items);
        }
    }
}
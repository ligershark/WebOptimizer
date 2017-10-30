using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
            var asset = new Asset("", "", new[] { "" });

            Assert.Equal(0, asset.Items.Count);
            asset.UseContentRoot();
            Assert.Equal(1, asset.Items.Count);
        }

        [Fact2]
        public void UseFileProvider_Success()
        {
            var minifier = new UseContentRoot();
            var asset = new Asset("", "", new[] { "" });

            Assert.Equal(0, asset.Items.Count);
            asset.UseFileProvider(null);
            Assert.Equal(1, asset.Items.Count);
        }
    }
}

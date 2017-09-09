using Microsoft.Extensions.DependencyInjection;
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

            Assert.False(asset.IsUsingContentRoot());
            asset.UseContentRoot();
            Assert.True(asset.IsUsingContentRoot());
        }
    }
}

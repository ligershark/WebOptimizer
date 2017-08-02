using Xunit;

namespace WebOptimizer.Test
{
    public class AssetMiddlewareOptionsTest
    {
        [Fact2]
        public void EnableCaching_InDevelopment_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var options = new AssetMiddlewareOptions(env);

            Assert.False(options.EnableCaching);
        }

        [Fact2]
        public void EnableCaching_NotInDevelopment_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "_Development" };
            var options = new AssetMiddlewareOptions(env);

            Assert.True(options.EnableCaching);
        }
    }
}

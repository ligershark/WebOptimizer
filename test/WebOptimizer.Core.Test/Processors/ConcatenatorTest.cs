using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class ConcatenatorTest
    {
        [Fact2]
        public async Task Concatinate_MultipleSources_Success()
        {
            var processor = new Concatenator();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> {
                { "/route1", "content".AsByteArray() },
                { "/route2", "content".AsByteArray() }
            };

            await processor.ExecuteAsync(context.Object);

            Assert.Equal(1, context.Object.Content.Count);
        }

        [Fact2]
        public async Task Concatinate_NoSources_Success()
        {
            var processor = new Concatenator();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]>();

            await processor.ExecuteAsync(context.Object);

            Assert.Equal(1, context.Object.Content.Count);
        }

        [Fact2]
        public void AddConcatinate_Assets_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("/file1", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("/file2", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            var assets = pipeline.AddBundle(new[] { asset1, asset2 });

            assets = assets.Concatinate();

            Assert.Equal(2, assets.Count());

            foreach (var asset in assets)
            {
                Assert.Equal(1, asset.Processors.Count);
                Assert.True(asset.Processors.First() is Concatenator);
            }
        }
    }
}

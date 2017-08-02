using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUglify.Css;
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
            context.Object.Content = new Dictionary<string, string> {
                { "/route1", "content" },
                { "/route2", "content" }
            };

            await processor.ExecuteAsync(context.Object);

            Assert.Equal(1, context.Object.Content.Count);
        }

        [Fact2]
        public async Task Concatinate_NoSources_Success()
        {
            var processor = new Concatenator();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, string>();

            await processor.ExecuteAsync(context.Object);

            Assert.Equal(1, context.Object.Content.Count);
        }
    }
}

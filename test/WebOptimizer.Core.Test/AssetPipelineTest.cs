using System;
using System.Linq;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetPipelineTest
    {
        [Fact2]
        public void AddSingeAsset_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = new Asset("/route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();

            pipeline.AddBundle(asset);

            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Theory2]
        [InlineData("route", "/route")]
        [InlineData("/route", "/route")]
        [InlineData("~/route", "/route")]
        [InlineData("~/route ", "/route")]
        [InlineData(" ~/route", "/route")]
        [InlineData(" ~/route ", "/route")]
        public void AddBundle_Success(string inputRoute, string normalizedRoute)
        {
            var asset = new Asset(inputRoute, "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.AddBundle(asset);

            Assert.Equal(normalizedRoute, pipeline.Assets.First().Route);
        }

        [Fact2]
        public void AddTwoAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = new Asset("/route1", "text/css", new[] { "file.css" });
            var asset2 = new Asset("/route2", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();

            pipeline.AddBundle(new[] { asset1, asset2 });

            Assert.Equal(2, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddTwoSameRoutes_Throws()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = new Asset("/route", "text/css", new[] { "file.css" });
            var asset2 = new Asset("/route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();

            var ex = Assert.Throws<ArgumentException>(() => pipeline.AddBundle(new[] { asset1, asset2 }));

            Assert.Equal("route", ex.ParamName);
            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddZeroSourceFiles_Fail()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            IAsset asset = new Asset("/file.css", "text/css", new string[0]);
            var pipeline = new AssetPipeline();

            asset = pipeline.AddBundle(asset);

            Assert.Equal(1, asset.SourceFiles.Count());
            Assert.Equal(asset.Route, asset.SourceFiles.First());
        }

        [Theory2]
        [InlineData("~/slash", "/slash")]
        [InlineData("~/slash", "slash")]
        [InlineData("~/slash", "~/slash")]
        [InlineData("/slash", "/slash")]
        [InlineData("/slash", "slash")]
        [InlineData("/slash", "~/slash")]
        [InlineData("noslash", "/noslash")]
        [InlineData("noslash", "noslash")]
        [InlineData("noslash", "~/noslash")]
        public void FromRoute_MixedSlashes_Success(string routeToAdd, string routeToCheck)
        {
            var pipeline = new AssetPipeline();
            pipeline.AddBundle(routeToAdd, "text/css", "file.css");

            Assert.True(pipeline.TryGetAssetFromRoute(routeToCheck, out var a1), routeToCheck);
        }

        [Theory2]
        [InlineData("~/1", "/2")]
        [InlineData("~/1", "2")]
        [InlineData("~/1", "~/2")]
        [InlineData("/1", "/2")]
        [InlineData("/1", "2")]
        [InlineData("/1", "~/2")]
        [InlineData("1", "/2")]
        [InlineData("1", "2")]
        [InlineData("1", "~/2")]
        public void FromRoute_NotFound(string routeToAdd, string routeToCheck)
        {
            var pipeline = new AssetPipeline();
            pipeline.AddBundle(routeToAdd, "text/css", "file.css");

            Assert.False(pipeline.TryGetAssetFromRoute(routeToCheck, out var a1), routeToCheck);
        }

        [Theory2]
        [InlineData("css/*.css", "/css/ost.css")]
        [InlineData("css/**/*.css", "/css/a/b/c/ost.css")]
        [InlineData("css/**/*.css", "css/a/b/c/ost.css")]
        [InlineData("**/*.css", "/css/a/b/c/ost.css")]
        [InlineData("*.css", "foo.css")]
        [InlineData("*.css", "/foo.css")]
        public void FromRoute_Globbing_Success(string pattern, string path)
        {
            var pipeline = new AssetPipeline();
            pipeline.AddFiles("text/css", pattern);

            Assert.True(pipeline.TryGetAssetFromRoute(path, out var a1));
            Assert.Equal($"/{path.TrimStart('/')}", a1.Route);
        }
    }
}

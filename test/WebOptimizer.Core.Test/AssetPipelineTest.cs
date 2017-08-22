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
        public void AddBundleWithGlobRoute_Throws()
        {
            var pipeline = new AssetPipeline();
            string route = "/*.css";
            var ex = Assert.Throws<ArgumentException>(() => pipeline.AddBundle(route, "text/css", new[] { "source.css" }));

            Assert.Equal("route", ex.ParamName);
            Assert.Equal(0, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddZeroSourceFilesToBundle_Fail()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            IAsset asset = new Asset("/file.css", "text/css", new string[0]);
            var pipeline = new AssetPipeline();

            Assert.Throws<ArgumentException>(() => pipeline.AddBundle(asset));
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

        [Fact2]
        public void AddFilesNoContentType_Throws()
        {
            var pipeline = new AssetPipeline();
            string ct = null;
            var ex = Assert.Throws<ArgumentException>(() => pipeline.AddFiles(ct, new[] { "file.css" }));

            Assert.Equal("contentType", ex.ParamName);
            Assert.Equal(0, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddFilesNoSourceFiles_Throws()
        {
            var pipeline = new AssetPipeline();
            var sourceFiles = new string[0];
            var ex = Assert.Throws<ArgumentException>(() => pipeline.AddFiles("text/css", sourceFiles));

            Assert.Equal("sourceFiles", ex.ParamName);
            Assert.Equal(0, pipeline.Assets.Count);
        }
    }
}

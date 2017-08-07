using System;
using System.Linq;
using NUglify.Css;
using NUglify.JavaScript;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetPipelineTest
    {
        [Fact2]
        public void DefaultValuesAsExpected()
        {
            var pipeline = new AssetPipeline();

            Assert.Equal(false, pipeline.EnableTagHelperBundling.HasValue);
            Assert.Null(pipeline.FileProvider);
        }

        [Fact2]
        public void AddSingeAsset_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("/route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.AddBundle(asset);

            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Theory2]
        [InlineData("route", "/route")]
        [InlineData("/route", "/route")]
        [InlineData("~/route", "/route")]
        public void AddBundle_Success(string inputRoute, string normalizedRoute)
        {
            var asset = Asset.Create(inputRoute, "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.AddBundle(asset);

            Assert.Equal(normalizedRoute, pipeline.Assets.First().Route);
        }

        [Fact2]
        public void AddTwoAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("/route1", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("/route2", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.AddBundle(new[] { asset1, asset2 });

            Assert.Equal(2, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddTwoSameRoutes_Throws()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("/route", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("/route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            var ex = Assert.Throws<ArgumentException>(() => pipeline.AddBundle(new[] { asset1, asset2 }));

            Assert.Equal(ex.ParamName, "route");
            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddRouteWithNoLeadingSlash_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route", "text/css", new[] { "file.css" });

            Assert.Equal("/route", asset1.Route);
        }

        [Fact2]
        public void AddZeroSourceFiles_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("/file.css", "text/css", new string[0]);
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

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

        [Fact2]
        public void AddJs_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddJavaScriptBundle("/foo.js", "file1.js", "file2.js");

            Assert.Equal("/foo.js", asset.Route);
            Assert.Equal("application/javascript; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);
        }

        [Fact2]
        public void AddJs_CustomSettings_Success()
        {
            var settings = new CodeSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddJavaScriptBundle("/foo.js", settings, "file1.js", "file2.js");

            Assert.Equal("/foo.js", asset.Route);
            Assert.Equal("application/javascript; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);

        }

        [Fact2]
        public void AddCss_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCssBundle("/foo.css", "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(4, asset.Processors.Count);
        }

        [Fact2]
        public void AddCss_CustomSettings_Success()
        {
            var settings = new CssSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCssBundle("/foo.css", settings, "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(4, asset.Processors.Count);
        }
    }
}

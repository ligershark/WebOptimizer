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
        public void AddSingeAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("/route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.Add(asset);

            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddTwoAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("/route1", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("/route2", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.Add(new[] { asset1, asset2 });

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

            var ex = Assert.Throws<ArgumentException>(() => pipeline.Add(new[] { asset1, asset2 }));

            Assert.Equal(ex.ParamName, "route");
            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddRouteWithNoLeadingSlash_Throws()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            var ex = Assert.Throws<ArgumentException>(() => pipeline.Add(new[] { asset1 }));
        }

        [Fact2]
        public void AddZeroSourceFiles_Success()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("/file.css", "text/css", new string[0]);
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            asset = pipeline.Add(asset);

            Assert.Equal(1, asset.SourceFiles.Count());
            Assert.Equal(asset.Route, asset.SourceFiles.First());
        }

        [Fact2]
        public void FromRoute_MixedSlashes_Success()
        {
            var pipeline = new AssetPipeline();
            pipeline.Add("/route1", "text/css", "file.css");

            Assert.True(pipeline.TryFromRoute("/route1", out var a1));
            Assert.False(pipeline.TryFromRoute("route1", out var a2));
        }

        [Fact2]
        public void AddJs_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddJs("/foo.js", "file1.js", "file2.js");

            Assert.Equal("/foo.js", asset.Route);
            Assert.Equal("application/javascript", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);
        }

        [Fact2]
        public void AddJs_CustomSettings_Success()
        {
            var settings = new CodeSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddJs("/foo.js", settings, "file1.js", "file2.js");

            Assert.Equal("/foo.js", asset.Route);
            Assert.Equal("application/javascript", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);

        }

        [Fact2]
        public void AddCss_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCss("/foo.css", "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(4, asset.Processors.Count);
        }

        [Fact2]
        public void AddCss_CustomSettings_Success()
        {
            var settings = new CssSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddCss("/foo.css", settings, "file1.css", "file2.css");

            Assert.Equal("/foo.css", asset.Route);
            Assert.Equal("text/css", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(4, asset.Processors.Count);
        }
    }
}

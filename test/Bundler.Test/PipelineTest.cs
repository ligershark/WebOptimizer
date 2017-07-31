using System;
using Xunit;

namespace Bundler.Test
{
    public class PipelineTest
    {
        [Fact2]
        public void DefaultValuesAsExpected()
        {
            var pipeline = new AssetPipeline();

            Assert.Equal(false, pipeline.EnableCaching.HasValue);
            Assert.Equal(false, pipeline.EnabledBundling.HasValue);
            Assert.Null(pipeline.FileProvider);
        }

        [Fact2]
        public void CachingDisabledInDevelopment()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };

            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            Assert.Equal(false, pipeline.EnableCaching.Value);
        }

        [Fact2]
        public void AddSingeAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.Add(asset);

            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddTwoAsset_Succes()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route1", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("route2", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            pipeline.Add(new[] { asset1, asset2 });

            Assert.Equal(2, pipeline.Assets.Count);
        }

        [Fact2]
        public void AddTwoSameRoutes_Throws()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("route", "text/css", new[] { "file.css" });
            var pipeline = new AssetPipeline();
            pipeline.EnsureDefaults(env);

            var ex = Assert.Throws<ArgumentException>(() => pipeline.Add(new[] { asset1, asset2 }));

            Assert.Equal(ex.ParamName, "route");
            Assert.Equal(1, pipeline.Assets.Count);
        }

        [Fact2]
        public void FromRoute_MixedSlashes_Success()
        {
            var pipeline = new AssetPipeline();
            pipeline.Add("/route1", "text/css", "file.css");
            pipeline.Add("route2", "text/css", "file.css");

            Assert.True(pipeline.TryFromRoute("/route1", out var a1));
            Assert.True(pipeline.TryFromRoute("route1", out var a2));
            Assert.True(pipeline.TryFromRoute("/route2", out var a3));
            Assert.True(pipeline.TryFromRoute("route2", out var a4));
        }
    }
}

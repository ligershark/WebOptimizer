using System;
using Xunit;

namespace Bundler.Test
{
    public class PipelineTest
    {
        [Fact2]
        public void CachingDisabledInDevelopment()
        {
            var env = new HostingEnvironment { EnvironmentName = "Development" };

            var pipeline = new Pipeline(env);

            Assert.Equal(false, pipeline.EnableCaching);
        }

        [Fact2]
        public void AddSingeAsset_Succes()
        {
            AssetManager.Assets.Clear();
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset = Asset.Create("route", "text/css", new[] { "file.css" });
            var pipeline = new Pipeline(env);

            pipeline.Add(asset);

            Assert.Equal(1, AssetManager.Assets.Count);
        }

        [Fact2]
        public void AddTwoAsset_Succes()
        {
            AssetManager.Assets.Clear();
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route1", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("route2", "text/css", new[] { "file.css" });
            var pipeline = new Pipeline(env);

            pipeline.Add(new[] { asset1, asset2 });

            Assert.Equal(2, AssetManager.Assets.Count);
        }

        [Fact2]
        public void AddTwoSameRoutes_Throws()
        {
            AssetManager.Assets.Clear();
            var env = new HostingEnvironment { EnvironmentName = "Development" };
            var asset1 = Asset.Create("route", "text/css", new[] { "file.css" });
            var asset2 = Asset.Create("route", "text/css", new[] { "file.css" });
            var pipeline = new Pipeline(env);

            var ex = Assert.Throws<ArgumentException>(() => pipeline.Add(new[] { asset1, asset2 }));

            Assert.Equal(ex.ParamName, "route");
            Assert.Equal(1, AssetManager.Assets.Count);
        }
    }
}

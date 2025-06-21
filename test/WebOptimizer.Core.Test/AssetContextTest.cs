using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace WebOptimizer.Core.Test;

public class AssetContextTest
{
    [Fact2]
    public void AssetContextConstructor_NullAsset()
    {
        var httpContext = new DefaultHttpContext();

        _ = Assert.Throws<ArgumentNullException>(() => new AssetContext(httpContext, null!, null!));
    }

    [Fact2]
    public void AssetContextConstructor_NullHttpContext()
    {
        string route = "route";
        string contentType = "text/css";
        string[] sourcefiles = ["file1.css"];
        var httpContext = new DefaultHttpContext();
        var logger = new Mock<ILogger<Asset>>();

        var asset = new Asset(route, contentType, sourcefiles, logger.Object);

        _ = Assert.Throws<ArgumentNullException>(() => new AssetContext(null!, asset, null!));
    }

    [Fact2]
    public void AssetContextConstructor_Success()
    {
        string route = "route";
        string contentType = "text/css";
        string[] sourcefiles = ["file1.css"];
        var httpContext = new DefaultHttpContext();
        var logger = new Mock<ILogger<Asset>>();

        var asset = new Asset(route, contentType, sourcefiles, logger.Object);
        var assetContext = new AssetContext(httpContext, asset, new WebOptimizerOptions());

        Assert.Equal(asset, assetContext.Asset);
        Assert.Equal(httpContext, assetContext.HttpContext);
        Assert.Empty(assetContext.Content);
    }
}

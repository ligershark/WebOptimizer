﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUglify.JavaScript;
using WebOptimizer.Processors;
using Xunit;

namespace WebOptimizer.Test.Processors;

public class JavaScriptMinifierTest
{
    [Fact2]
    public async Task MinifyJs_DefaultSettings_Success()
    {
        var minifier = new JavaScriptMinifier(new JsSettings());
        var context = new Mock<IAssetContext>().SetupAllProperties();
        context.Object.Content = new Dictionary<string, byte[]> { { "", "var i = 0;".AsByteArray() } };
        var options = new Mock<WebOptimizerOptions>();

        await minifier.ExecuteAsync(context.Object);

        Assert.Equal("var i=0", context.Object.Content.First().Value.AsString());
        Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
    }

    [Theory2]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("// comment")]
    [InlineData("   // ")]
    [InlineData("\r\n  \t \r \n")]
    public async Task MinifyJs_EmptyContent_Success(string input)
    {
        var minifier = new JavaScriptMinifier(new JsSettings());
        var context = new Mock<IAssetContext>().SetupAllProperties();
        context.Object.Content = new Dictionary<string, byte[]> { { "", input.AsByteArray() } };
        var options = new Mock<WebOptimizerOptions>();

        await minifier.ExecuteAsync(context.Object);

        Assert.Equal("", context.Object.Content.First().Value.AsString());
        Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
    }

    [Fact2]
    public async Task MinifyJs_CustomSettings_Success()
    {
        var settings = new JsSettings(new CodeSettings { TermSemicolons = true});
        var minifier = new JavaScriptMinifier(settings);
        var context = new Mock<IAssetContext>().SetupAllProperties();
        context.Object.Content = new Dictionary<string, byte[]> { { "", "var i = 0;".AsByteArray() } };
        var options = new Mock<WebOptimizerOptions>();

        await minifier.ExecuteAsync(context.Object);

        Assert.Equal("var i=0;", context.Object.Content.First().Value.AsString());
        Assert.Equal("", minifier.CacheKey(new DefaultHttpContext(), context.Object));
    }

    [Fact2]
    public void AddJsBundle_DefaultSettings_Success()
    {
        var logger = new Mock<ILogger<Asset>>();
        var pipeline = new AssetPipeline(logger.Object);
        var asset = pipeline.AddJavaScriptBundle("/foo.js", "file1.js", "file2.js");

        Assert.Equal("/foo.js", asset.Route);
        Assert.Equal("text/javascript; charset=UTF-8", asset.ContentType);
        Assert.Equal(2, asset.SourceFiles.Count);
        Assert.Equal(4, asset.Processors.Count);
    }

    [Fact2]
    public void AddJsBundle_DefaultSettings_SuccessRelative()
    {
        var logger = new Mock<ILogger<Asset>>();
        var pipeline = new AssetPipeline(logger.Object);
        var asset = pipeline.AddJavaScriptBundle("foo.js", "file1.js", "file2.js");

        Assert.Equal("/foo.js", asset.Route);
        Assert.Equal("text/javascript; charset=UTF-8", asset.ContentType);
        Assert.Equal(2, asset.SourceFiles.Count);
        Assert.Equal(4, asset.Processors.Count);
    }


    [Fact2]
    public void AddJsBundle_CustomSettings_Success()
    {
        var settings = new JsSettings();
        var logger = new Mock<ILogger<Asset>>();
        var pipeline = new AssetPipeline(logger.Object);
        var asset = pipeline.AddJavaScriptBundle("/foo.js", settings, "file1.js", "file2.js");

        Assert.Equal("/foo.js", asset.Route);
        Assert.Equal("text/javascript; charset=UTF-8", asset.ContentType);
        Assert.Equal(2, asset.SourceFiles.Count);
        Assert.Equal(4, asset.Processors.Count);
    }

    [Fact2]
    public void AddJsFiles_DefaultSettings_Success()
    {
        var logger = new Mock<ILogger<Asset>>();
        var pipeline = new AssetPipeline(logger.Object);
        var asset = pipeline.MinifyJsFiles().First();

        Assert.Equal("/**/*.js", asset.Route);
        Assert.Equal("text/javascript; charset=UTF-8", asset.ContentType);
        Assert.Single(asset.SourceFiles);
        Assert.Equal(2, asset.Processors.Count);
    }
}

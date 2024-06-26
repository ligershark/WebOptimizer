using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using WebOptimizer.Taghelpers;
using Xunit;

namespace WebOptimizer.Core.Test.TagHelpers
{
    public class LinkTagHelperTest
    {
        [Fact2]
        public void AddCdn_Success()
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);
            var cache = new Mock<IMemoryCache>();
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);

            context.Setup(c => c.RequestServices.GetService(typeof(IMemoryCache)))
                .Returns(cache.Object);
            
            var options = new WebOptimizerOptions
            {
                EnableTagHelperBundling = true,
                CdnUrl = "https://my-cdn.com"
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            
            var sources = new List<IOptionsChangeTokenSource<WebOptimizerOptions>>();
            var optionsMonitorCache = new Mock<IOptionsMonitorCache<WebOptimizerOptions>>();
            
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, sources, optionsMonitorCache.Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("route");
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file.css"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>(), It.IsAny<IWebOptimizerOptions>()))
                .Returns("abc123");
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(It.IsAny<string>(), out assetObject)).Returns(true);
            
            var linkTagHelper = new LinkTagHelper(env.Object, cache.Object, assetPipeline.Object, optionsMonitor.Object);
            var viewContext = new ViewContext
            {
                HttpContext = context.Object
            };
            linkTagHelper.ViewContext = viewContext;
            linkTagHelper.CurrentViewContext = viewContext;

            var tagHelperContext = new Mock<TagHelperContext>(
                "link",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "unique");
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", "route") };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var hrefAttr = tagHelperOutput.Attributes.First(x => x.Name == "href");
            Assert.StartsWith(options.CdnUrl, hrefAttr.Value.ToString());
        }
        
        [Fact2]
        public void AddCdn_TagHelperBundlingDisabled_Success()
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);
            var cache = new Mock<IMemoryCache>();
            object cacheValue = "file1.css?v=abc123";
            cache.Setup(c => c.TryGetValue("file1.css", out cacheValue)).Returns(true);
            object cacheValue2 = "file2.css?v=def456";
            cache.Setup(c => c.TryGetValue("file2.css", out cacheValue2)).Returns(true);
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);

            context.Setup(c => c.RequestServices.GetService(typeof(IMemoryCache)))
                .Returns(cache.Object);
            
            var options = new WebOptimizerOptions
            {
                EnableTagHelperBundling = false,
                CdnUrl = "https://my-cdn.com"
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            
            var sources = new List<IOptionsChangeTokenSource<WebOptimizerOptions>>();
            var optionsMonitorCache = new Mock<IOptionsMonitorCache<WebOptimizerOptions>>();
            
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, sources, optionsMonitorCache.Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns("route");
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file1.css", "file2.css"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(It.IsAny<string>(), out assetObject)).Returns(true);
            
            var linkTagHelper = new LinkTagHelper(env.Object, cache.Object, assetPipeline.Object, optionsMonitor.Object);
            var viewContext = new ViewContext
            {
                HttpContext = context.Object
            };
            linkTagHelper.ViewContext = viewContext;
            linkTagHelper.CurrentViewContext = viewContext;

            var tagHelperContext = new Mock<TagHelperContext>(
                "link",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "unique");
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", "route") };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            string[] linkTags = tagHelperOutput.PostElement.GetContent().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            Assert.All(linkTags, s => Assert.StartsWith($"<link href=\"{options.CdnUrl}", s));
        }
    }
}
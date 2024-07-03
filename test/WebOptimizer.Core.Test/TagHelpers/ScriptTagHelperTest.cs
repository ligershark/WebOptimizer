using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using WebOptimizer.Taghelpers;
using Xunit;

namespace WebOptimizer.Core.Test.TagHelpers
{
    public class ScriptTagHelperTest
    {
        [Theory2]
        [InlineData("https://my-cdn.com", "")]
        [InlineData("https://my-cdn.com", "/myapp")]
        [InlineData("", "")]
        [InlineData("", "/myapp")]
        public void CdnUrl_RouteIsAsset_Success(string cdnUrl, string pathBase)
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
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);
            
            var options = new WebOptimizerOptions
            {
                EnableTagHelperBundling = true,
                CdnUrl = cdnUrl
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            
            var sources = new List<IOptionsChangeTokenSource<WebOptimizerOptions>>();
            var optionsMonitorCache = new Mock<IOptionsMonitorCache<WebOptimizerOptions>>();
            
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, sources, optionsMonitorCache.Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);

            var route = "/testbundle";
            var cacheKey = "abc123";
            
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/javascript");
            asset.SetupGet(a => a.Route).Returns(route);
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file.js"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>(), It.IsAny<IWebOptimizerOptions>()))
                .Returns(cacheKey);
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(route, out assetObject)).Returns(true);
            
            var scriptTagHelper =
                new ScriptTagHelper(env.Object, cache.Object, assetPipeline.Object, optionsMonitor.Object);
            var viewContext = new ViewContext
            {
                HttpContext = context.Object
            };
            scriptTagHelper.ViewContext = viewContext;
            scriptTagHelper.CurrentViewContext = viewContext;

            var tagHelperContext = new Mock<TagHelperContext>(
                "script",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "unique");
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("src", route) };
            
            var tagHelperOutput = new TagHelperOutput("script", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            
            scriptTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var srcAttr = tagHelperOutput.Attributes.First(x => x.Name == "src");
            Assert.Equal($"{options.CdnUrl}{pathBase}{route}?v={cacheKey}", srcAttr.Value.ToString());
        }
        
        [Theory2]
        [InlineData("https://my-cdn.com", "")]
        [InlineData("https://my-cdn.com", "/myapp")]
        [InlineData("", "")]
        [InlineData("", "/myapp")]
        public void CdnUrl_RouteIsNotAsset_Success(string cdnUrl, string pathBase)
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
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);
            
            var options = new WebOptimizerOptions
            {
                EnableTagHelperBundling = true,
                CdnUrl = cdnUrl
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            
            var sources = new List<IOptionsChangeTokenSource<WebOptimizerOptions>>();
            var optionsMonitorCache = new Mock<IOptionsMonitorCache<WebOptimizerOptions>>();
            
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, sources, optionsMonitorCache.Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            IAsset asset;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(It.IsAny<string>(), out asset)).Returns(false);
            
            var scriptTagHelper =
                new ScriptTagHelper(env.Object, cache.Object, assetPipeline.Object, optionsMonitor.Object);
            var viewContext = new ViewContext
            {
                HttpContext = context.Object
            };
            scriptTagHelper.ViewContext = viewContext;
            scriptTagHelper.CurrentViewContext = viewContext;

            var tagHelperContext = new Mock<TagHelperContext>(
                "script",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "unique");
            var srcValue = $"{pathBase}/lib/bootstrap/dist/js/bootstrap.min.js";
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("src", srcValue) };
            
            var tagHelperOutput = new TagHelperOutput("script", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            
            scriptTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var srcAttr = tagHelperOutput.Attributes.First(x => x.Name == "src");
            Assert.Equal($"{options.CdnUrl}{srcValue}", srcAttr.Value.ToString());
        }
        
        [Theory2]
        [InlineData("https://my-cdn.com", "")]
        [InlineData("https://my-cdn.com", "/myapp")]
        [InlineData("", "")]
        [InlineData("", "/myapp")]
        public void CdnUrl_RouteIsAsset_TagHelperBundlingDisabled_Success(string cdnUrl, string pathBase)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);
            var cache = new Mock<IMemoryCache>();
            object cacheValue = "/file1.js?v=abc123";
            cache.Setup(c => c.TryGetValue("file1.js", out cacheValue)).Returns(true);
            object cacheValue2 = "/file2.js?v=def456";
            cache.Setup(c => c.TryGetValue("file2.js", out cacheValue2)).Returns(true);
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";
            
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);
            context.Setup(c => c.RequestServices.GetService(typeof(IMemoryCache)))
                .Returns(cache.Object);
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);
            
            var options = new WebOptimizerOptions
            {
                EnableTagHelperBundling = false,
                CdnUrl = cdnUrl
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            
            var sources = new List<IOptionsChangeTokenSource<WebOptimizerOptions>>();
            var optionsMonitorCache = new Mock<IOptionsMonitorCache<WebOptimizerOptions>>();
            
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, sources, optionsMonitorCache.Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);

            var route = "/testbundle";
            var asset = new Mock<IAsset>().SetupAllProperties();
            asset.SetupGet(a => a.ContentType).Returns("text/javascript");
            asset.SetupGet(a => a.Route).Returns(route);
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file1.js", "file2.js"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(route, out assetObject)).Returns(true);
            
            var scriptTagHelper = new ScriptTagHelper(env.Object, cache.Object, assetPipeline.Object, optionsMonitor.Object);
            var viewContext = new ViewContext
            {
                HttpContext = context.Object
            };
            scriptTagHelper.ViewContext = viewContext;
            scriptTagHelper.CurrentViewContext = viewContext;

            var tagHelperContext = new Mock<TagHelperContext>(
                "script",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "unique");
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("src", route) };
            
            var tagHelperOutput = new TagHelperOutput("scripts", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            scriptTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            string[] scriptTags = tagHelperOutput.PostElement.GetContent().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, scriptTags.Length);
            Assert.Contains($"src=\"{options.CdnUrl}{pathBase}{cacheValue}\"", scriptTags[0]);
            Assert.Contains($"src=\"{options.CdnUrl}{pathBase}{cacheValue2}\"", scriptTags[1]);
        }
    }
}
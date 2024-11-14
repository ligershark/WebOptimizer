using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
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
    public class LinkTagHelperTest
    {

        public static IEnumerable<object[]> GetUrls()
        {
            yield return new object[] { "/test.css" };
            yield return new object[] { new HtmlString("https://host.com/path?query&amp;parameter=val") };
        }


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
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns(route);
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file.css"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            asset.Setup(a => a.GenerateCacheKey(It.IsAny<HttpContext>(), It.IsAny<IWebOptimizerOptions>()))
                .Returns(cacheKey);
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(route, out assetObject)).Returns(true);
            
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
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", route) };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var hrefAttr = tagHelperOutput.Attributes.First(x => x.Name == "href");
            Assert.Equal($"{options.CdnUrl}{pathBase}{route}?v={cacheKey}", hrefAttr.Value.ToString());
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
            var hrefValue = $"{pathBase}/lib/bootstrap/dist/css/bootstrap.min.css";
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", hrefValue) };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var hrefAttr = tagHelperOutput.Attributes.First(x => x.Name == "href");
            Assert.Equal($"{options.CdnUrl}{hrefValue}", hrefAttr.Value.ToString());
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
            object cacheValue = "/file1.css?v=abc123";
            cache.Setup(c => c.TryGetValue("file1.css", out cacheValue)).Returns(true);
            object cacheValue2 = "/file2.css?v=def456";
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
            asset.SetupGet(a => a.ContentType).Returns("text/css");
            asset.SetupGet(a => a.Route).Returns(route);
            asset.SetupGet(a => a.SourceFiles).Returns(new HashSet<string>(new []{"file1.css", "file2.css"}));
            asset.SetupGet(a => a.ExcludeFiles).Returns(new List<string>());
            asset.SetupGet(a => a.Items).Returns(new Dictionary<string, object>{ {"fileprovider", fileProvider.Object}});
            var assetObject = asset.Object;
            var assetPipeline = new Mock<IAssetPipeline>();
            assetPipeline.Setup(ap => ap.TryGetAssetFromRoute(route, out assetObject)).Returns(true);
            
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
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", route) };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            string[] linkTags = tagHelperOutput.PostElement.GetContent().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, linkTags.Length);
            Assert.Contains($"href=\"{options.CdnUrl}{pathBase}{cacheValue}\"", linkTags[0]);
            Assert.Contains($"href=\"{options.CdnUrl}{pathBase}{cacheValue2}\"", linkTags[1]);            
        }
        
        [Theory2]
        [InlineData("//google.com/test.css")]
        [InlineData("http://google.com/test.css")]
        [InlineData("https://google.com/test.css")]
        public void AbsoluteUrl_RouteIsNotAsset_DoesNotAddCdnOrPath(string absoluteUrl)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);
            
            var options = new WebOptimizerOptions
            {
                CdnUrl = "https://mycdn.com"
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, new List<IOptionsChangeTokenSource<WebOptimizerOptions>>(), new Mock<IOptionsMonitorCache<WebOptimizerOptions>>().Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            var linkTagHelper = new LinkTagHelper(env.Object, new Mock<IMemoryCache>().Object,
                new Mock<IAssetPipeline>().Object, optionsMonitor.Object);
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";
            
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);
            var pathBase = "/myApp";
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);
            
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
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", absoluteUrl) };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var hrefValue = tagHelperOutput.Attributes.First(x => x.Name == "href").Value;
            Assert.Equal(absoluteUrl, hrefValue);
        }


        [Theory2]
        [MemberData(nameof(GetUrls))]
        public void RouteIsNotAsset_DoesNotChangeAttributeType(object route)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);

            var options = new WebOptimizerOptions
            {
                CdnUrl = "https://mycdn.com"
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, new List<IOptionsChangeTokenSource<WebOptimizerOptions>>(), new Mock<IOptionsMonitorCache<WebOptimizerOptions>>().Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            var linkTagHelper = new LinkTagHelper(env.Object, new Mock<IMemoryCache>().Object, 
                new Mock<IAssetPipeline>().Object, optionsMonitor.Object);
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";

            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);
            var pathBase = "/myApp";
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);

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

            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", route) };

            var tagHelperOutput = new TagHelperOutput("scripts", attributes, (useCachedResult, encoder) => Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            object hrefValue = tagHelperOutput.Attributes.First(x => x.Name == "href").Value;
            Assert.IsType(route.GetType(), hrefValue);
            Assert.Contains(route.ToString(), hrefValue.ToString());
        }

        [Fact2]
        public void RelativeUrl_RouteIsNotAsset_DoesAddCdnAndPath()
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(fi => fi.Exists).Returns(true);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootFileProvider).Returns(fileProvider.Object);
            
            var options = new WebOptimizerOptions
            {
                CdnUrl = "https://mycdn.com"
            };
            var optionsFactory = new Mock<IOptionsFactory<WebOptimizerOptions>>();
            optionsFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(options);
            var optionsMonitor = new Mock<OptionsMonitor<WebOptimizerOptions>>(optionsFactory.Object, new List<IOptionsChangeTokenSource<WebOptimizerOptions>>(), new Mock<IOptionsMonitorCache<WebOptimizerOptions>>().Object);
            optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            var linkTagHelper = new LinkTagHelper(env.Object, new Mock<IMemoryCache>().Object, new Mock<IAssetPipeline>().Object, optionsMonitor.Object);
            var context = new Mock<HttpContext>().SetupAllProperties();
            StringValues ae = "gzip, deflate";
            
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                .Returns(false)
                .Returns(true);
            context.Setup(c => c.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);
            var pathBase = "/myApp";
            context.SetupGet(c => c.Request.PathBase).Returns(pathBase);
            
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
            var relativeUrl = "/test.css";
            var attributes = new TagHelperAttributeList { new TagHelperAttribute("href", relativeUrl) };
            
            var tagHelperOutput = new TagHelperOutput("link", attributes, (useCachedResult, encoder) =>  Task.Factory.StartNew<TagHelperContent>(
                () => new DefaultTagHelperContent()));
            linkTagHelper.Process(tagHelperContext.Object, tagHelperOutput);
            var hrefValue = tagHelperOutput.Attributes.First(x => x.Name == "href").Value;
            Assert.Equal($"{options.CdnUrl}{pathBase}{relativeUrl}", hrefValue);
        }
    }
}
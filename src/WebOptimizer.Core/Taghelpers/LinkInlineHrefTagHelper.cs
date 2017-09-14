using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers
{

    /// <summary>
    /// Tag helper for inlining CSS
    /// </summary>
    [HtmlTargetElement("link", Attributes = "inline, href")]
    public class LinkInlineHrefTagHelper : BaseTagHelper
    {
        private IAssetBuilder _builder;

        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        public LinkInlineHrefTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsSnapshot<WebOptimizerOptions> options, IAssetBuilder builder)
            : base(env, cache, pipeline, options)
        {
            _builder = builder;
        }

        /// <summary>
        /// Makes sure this taghelper runs before the built in ones.
        /// </summary>
        public override int Order => base.Order + 1;

        /// <summary>
        /// Gets or sets the href attribute
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                return;
            }

            if (!string.IsNullOrEmpty(Href))
            {
                TagHelperAttribute scoped = GetScoped(output);
                output.TagName = "style";
                output.Attributes.Clear();

                // Make sure to use add the scope attribute if the user specified it
                if (scoped != null)
                {
                    output.Attributes.Add(scoped);
                }

                string route = AssetPipeline.NormalizeRoute(Href);
                string content = await GetFileContentAsync(route);

                output.Content.SetHtmlContent(content);
                output.TagMode = TagMode.StartTagAndEndTag;
            }
        }

        private TagHelperAttribute GetScoped(TagHelperOutput output)
        {
            if (output.Attributes.TryGetAttribute("scoped", out var attr))
            {
                return attr;
            }

            return null;
        }

        private async Task<string> GetFileContentAsync(string route)
        {
            if (Pipeline.TryGetAssetFromRoute(route, out IAsset asset))
            {
                IAssetResponse response = await _builder.BuildAsync(asset, ViewContext.HttpContext, Options);
                return response.Body.AsString();
            }

            string cacheKey = "_WO_" + route;

            if (Cache.TryGetValue(cacheKey, out string content))
            {
                return content;
            }

            string cleanRoute = route.TrimStart('~');
            string file = HostingEnvironment.WebRootFileProvider.GetFileInfo(cleanRoute).PhysicalPath;

            if (File.Exists(file))
            {
                using (StreamReader reader = File.OpenText(file))
                {
                    content = await reader.ReadToEndAsync();
                    AddToCache(cacheKey, content, HostingEnvironment.WebRootFileProvider, cleanRoute);

                    return content;
                }
            }

            throw new FileNotFoundException("File or bundle doesn't exist", route);
        }
    }
}

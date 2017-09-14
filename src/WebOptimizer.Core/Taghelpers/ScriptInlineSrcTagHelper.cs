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
    [HtmlTargetElement("script", Attributes = "inline")]
    public class ScriptInlineSrcTagHelper : BaseTagHelper
    {
        private IAssetBuilder _builder;

        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        public ScriptInlineSrcTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsSnapshot<WebOptimizerOptions> options, IAssetBuilder builder)
            : base(env, cache, pipeline, options)
        {
            _builder = builder;
        }

        /// <summary>
        /// Makes sure this taghelper runs before the built in ones.
        /// </summary>
        public override int Order => base.Order + 1;

        /// <summary>
        /// Gets or sets the src attribute
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                // output.SuppressOutput() was called by another TagHelper before this one
                return;
            }

            if (!string.IsNullOrEmpty(Src))
            {
                output.Attributes.RemoveAll("inline");
                output.Attributes.RemoveAll("integrity");
                output.Attributes.RemoveAll("language");
                output.Attributes.RemoveAll("src");
                output.Attributes.RemoveAll("async");
                output.Attributes.RemoveAll("defer");

                string route = AssetPipeline.NormalizeRoute(Src);
                string content = await GetFileContentAsync(route);

                output.Content.SetHtmlContent(content);
                output.TagMode = TagMode.StartTagAndEndTag;
            }
        }

        /// <summary>
        /// Gets the file content asynchronous.
        /// </summary>
        /// <exception cref="FileNotFoundException">File or bundle doesn't exist</exception>
        protected async Task<string> GetFileContentAsync(string route)
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

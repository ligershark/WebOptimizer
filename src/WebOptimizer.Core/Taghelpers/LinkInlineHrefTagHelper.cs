using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers
{

    /// <summary>
    /// Tag helper for inlining CSS
    /// </summary>
    [HtmlTargetElement("link", Attributes = "inline, href")]
    public class LinkInlineHrefTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        public LinkInlineHrefTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsSnapshot<WebOptimizerOptions> options)
            : base(env, cache, pipeline, options)
        { }

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

                Options.EnsureDefaults(HostingEnvironment);
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
            string cacheKey = route;

            if (Pipeline.TryGetAssetFromRoute(route, out IAsset asset))
            {
                cacheKey = asset.GenerateCacheKey(ViewContext.HttpContext);
            }

            if (Cache.TryGetValue(cacheKey, out MemoryCachedResponse response))
            {
                return response.Body.AsString();
            }

            if (asset != null)
            {
                byte[] contents = await asset.ExecuteAsync(ViewContext.HttpContext, Options);

                AddToCache(cacheKey, contents, asset.GetFileProvider(HostingEnvironment), asset.SourceFiles.ToArray());
                string s = contents.AsString();

                return s ?? $"/* File '{route}' not found */";
            }
            else
            {
                string file = HostingEnvironment.WebRootFileProvider.GetFileInfo(route.TrimStart('~')).PhysicalPath;

                if (File.Exists(file))
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] content = await fs.AsBytesAsync();
                        AddToCache(cacheKey, content, HostingEnvironment.WebRootFileProvider, file);

                        return content.AsString();
                    }
                }
            }

            throw new FileNotFoundException("File or bundle doesn't exist", route);
        }

        private void AddToCache(string cacheKey, byte[] value, IFileProvider fileProvider, params string[] files)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            foreach (string file in files)
            {
                cacheOptions.AddExpirationToken(fileProvider.Watch(file));
            }

            var response = new MemoryCachedResponse(200, value);

            Cache.Set(cacheKey, response, cacheOptions);
        }
    }
}

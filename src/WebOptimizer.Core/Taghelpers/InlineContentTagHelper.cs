using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer.Taghelpers
{

    /// <summary>
    /// Tag helper for inlining CSS
    /// </summary>
    [HtmlTargetElement("link", Attributes = "inline, href")]
    [HtmlTargetElement("script", Attributes = "inline, src")]
    public class InlineContentTagHelper : BaseTagHelper
    {
        private string _route;

        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        public InlineContentTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline)
            : base(env, cache, pipeline)
        { }

        /// <summary>
        /// Makes sure this taghelper runs before the built in ones.
        /// </summary>
        public override int Order => base.Order + 1;

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        public override async Task ProcessSafeAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (output.TagName.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                output.TagName = "style";
                ParseRoute(context.AllAttributes["href"].Value.ToString());
                output.Attributes.Clear();
            }
            else if (output.TagName.Equals("script", StringComparison.OrdinalIgnoreCase))
            {
                ParseRoute(context.AllAttributes["src"].Value.ToString());
                output.Attributes.RemoveAll("inline");
                output.Attributes.RemoveAll("integrity");
                output.Attributes.RemoveAll("language");
                output.Attributes.RemoveAll("src");
                output.Attributes.RemoveAll("async");
                output.Attributes.RemoveAll("defer");
            }

            Pipeline.EnsureDefaults(HostingEnvironment);
            string content = await GetFileContentAsync(_route);

            output.Content.SetHtmlContent(content);
            output.TagMode = TagMode.StartTagAndEndTag;
        }

        private void ParseRoute(string route)
        {
            int index = route.IndexOfAny(new[] { '?', '#' });

            if (index > -1)
            {
                _route = route.Substring(0, index);
            }
            else
            {
                _route = route;
            }
        }

        private async Task<string> GetFileContentAsync(string route)
        {
            string cacheKey = route;

            if (Pipeline.TryFromRoute(route, out IAsset asset))
            {
                cacheKey = asset.GenerateCacheKey(ViewContext.HttpContext);
            }

            if (Cache.TryGetValue(cacheKey, out byte[] value))
            {
                return value.AsString();
            }

            if (asset != null)
            {
                byte[] contents = await asset.ExecuteAsync(ViewContext.HttpContext);

                AddToCache(cacheKey, contents, asset.SourceFiles.ToArray());
                string s = contents.AsString();

                return s ?? $"/* File '{route}' not found */";
            }
            else
            {
                string file = Pipeline.FileProvider.GetFileInfo(route.TrimStart('~')).PhysicalPath;

                if (File.Exists(file))
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] content = await fs.AsBytesAsync();
                        AddToCache(cacheKey, content, file);

                        return content.AsString();
                    }
                }
            }

            throw new FileNotFoundException("File or bundle doesn't exist", route);
        }

        private void AddToCache(string cacheKey, byte[] value, params string[] files)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            foreach (string file in files)
            {
                cacheOptions.AddExpirationToken(Pipeline.FileProvider.Watch(file));
            }

            Cache.Set(cacheKey, value, cacheOptions);
        }
    }
}

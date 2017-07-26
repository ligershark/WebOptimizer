using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bundler.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Bundler.Taghelpers
{

    /// <summary>
    /// Tag helper for inlining CSS
    /// </summary>
    [HtmlTargetElement("link", Attributes = "inline, href")]
    [HtmlTargetElement("script", Attributes = "inline, src")]
    public class InlineContentTagHelper : TagHelper
    {
        private readonly FileCache _fileCache;
        private string _route;

        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        public InlineContentTagHelper(IHostingEnvironment env, IMemoryCache cache)
        {
            _fileCache = new FileCache(env.WebRootFileProvider, cache);
        }

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Orders before any built-in TagHelpers run.
        /// </summary>
        public override int Order => -2000;

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (output.TagName.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                output.TagName = "style";
                ParseRoute(output.Attributes["href"].Value.ToString());
            }
            else if (output.TagName.Equals("script", StringComparison.OrdinalIgnoreCase))
            {
                ParseRoute(output.Attributes["src"].Value.ToString());
            }

            string content = await GetFileContentAsync(_route);

            output.Content.SetHtmlContent(content);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Clear();
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
            IBundle bundle = GetBundle(route);
            string cacheKey = bundle == null ? route : BundleMiddleware.GetCacheKey(ViewContext.HttpContext, bundle);

            if (_fileCache.TryGetValue(cacheKey, out string value))
            {
                return value;
            }

            if (bundle != null)
            {
                string contents = await BundleMiddleware.ExecuteAsync(ViewContext.HttpContext, bundle, _fileCache.FileProvider).ConfigureAwait(false);

                _fileCache.Add(cacheKey, contents, bundle.SourceFiles);
                return contents;
            }
            else
            {
                string file = _fileCache.FileProvider.GetFileInfo(route).PhysicalPath;

                if (File.Exists(file))
                {
                    string contents = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                    _fileCache.Add(cacheKey, contents, file);

                    return contents;
                }
            }

            throw new FileNotFoundException("File or bundle doesn't exist", route);
        }

        private IBundle GetBundle(string route)
        {
            return Extensions.Options.Bundles.FirstOrDefault(t => t.Route.Equals(route));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bundler.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Bundler.Taghelpers
{

    /// <summary>
    /// Tag helper for inlining CSS
    /// </summary>
    [HtmlTargetElement("style", Attributes = InlineAttribute)]
    [HtmlTargetElement("script", Attributes = InlineAttribute)]
    public class InlineContentTagHelper : TagHelper
    {
        internal const string InlineAttribute = "inline";
        private readonly FileCache _fileCache;

        /// <summary>
        /// Tag helper for inlining content
        /// </summary>
        /// <param name="env"></param>
        /// <param name="cache"></param>
        public InlineContentTagHelper(IHostingEnvironment env, IMemoryCache cache)
        {
            _fileCache = new FileCache(env.WebRootFileProvider, cache);
        }

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override async void Process(TagHelperContext context, TagHelperOutput output)
        {
            if(context.AllAttributes.TryGetAttribute(InlineAttribute, out TagHelperAttribute attribute))
            {
                string route = attribute.Value.ToString();
                string css = await GetFileContentAsync(route);
                output.Content.SetHtmlContent(css);
                if (output.Attributes.Contains(attribute))
                {
                    output.Attributes.Remove(attribute);
                }
            }
        }

        private async System.Threading.Tasks.Task<string> GetFileContentAsync(string route)
        {
            if(_fileCache.TryGetValue(route, out string value))
            {
                return value;
            }
            else
            {
                IBundle bundle = GetBundle(route);

                if (bundle == null)
                {
                    var file = _fileCache.FileProvider.GetFileInfo(route).PhysicalPath;
                    if (file != null && File.Exists(file))
                    {
                        var contents = await File.ReadAllTextAsync(file);
                        _fileCache.AddFileToCache(route, contents, file);
                        return contents;
                    }
                }
            }

            throw new NotImplementedException();
        }

        private IBundle GetBundle(string route)
        {
            return Extensions.Options.Bundles.FirstOrDefault(t => t.Route.Equals(route));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// A TagHelper for hooking JavaScript bundles up to the HTML page.
    /// </summary>
    [HtmlTargetElement("script")]
    public class ScriptTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTagHelper"/> class.
        /// </summary>
        public ScriptTagHelper(IWebHostEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsMonitor<WebOptimizerOptions> options)
            : base(env, cache, pipeline, options)
        { }

          /// <summary>
        /// For HttpContext Access
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext CurrentViewContext { get; set; }
        
        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                return;
            }

            string src = LinkTagHelper.GetValue("src", output);

            if (string.IsNullOrEmpty(src))
                return;
                
            if (CurrentViewContext.HttpContext.Request.PathBase.HasValue)
                src = src.TrimStart(CurrentViewContext.HttpContext.Request.PathBase.Value.ToCharArray());

            if (Pipeline.TryGetAssetFromRoute(src, out IAsset asset) && !output.Attributes.ContainsName("inline"))
            {
                if (Options.EnableTagHelperBundling == true)
                {
                    if (CurrentViewContext.HttpContext.Request.PathBase.HasValue)
                        src = CurrentViewContext.HttpContext.Request.PathBase.Value + GenerateHash(asset);
                    else
                        src = GenerateHash(asset);

                    output.Attributes.SetAttribute("src", src);
                }
                else
                {
                    WriteIndividualTags(output, asset);
                }
            }

        }

        private void WriteIndividualTags(TagHelperOutput output, IAsset asset)
        {
            output.SuppressOutput();

            var attrs = new List<string>();

            foreach (TagHelperAttribute item in output.Attributes.Where(a => !a.Name.Equals("src", StringComparison.OrdinalIgnoreCase)))
            {
                string attr = item.Name;

                if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
                {
                    string quote = GetQuote(item.ValueStyle);
                    attr += "=" + quote + item.Value + quote;
                }

                attrs.Add(attr);
            }

            IEnumerable<string> sourceFiles = Asset.ExpandGlobs(asset, HostingEnvironment);

            foreach (string file in sourceFiles)
            {
                string src = AddFileVersionToPath(file, asset);
                output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>" + Environment.NewLine);
            }
        }
    }
}

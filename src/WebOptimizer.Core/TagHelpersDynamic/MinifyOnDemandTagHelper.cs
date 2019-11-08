using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUglify;
using WebOptimizer.Taghelpers;

namespace WebOptimizer.TagHelpersDynamic
{
    [HtmlTargetElement("style", Attributes = "minify")]
    [HtmlTargetElement("script", Attributes = "minify")]
    public class MinifyOnDemandTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
        /// </summary>
        public MinifyOnDemandTagHelper(IWebHostEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsMonitor<WebOptimizerOptions> options) 
            : base(env, cache, pipeline, options) { }

        /// <summary>
        /// Asynchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var input = (await output.GetChildContentAsync()).GetContent();

            UglifyResult result;
            result = context.TagName.Equals("script", StringComparison.InvariantCultureIgnoreCase) 
                ? Uglify.Js(input, ServiceExtensions.CodeBundlingSettings.CodeSettings) 
                : Uglify.Css(input, ServiceExtensions.CssBundlingSettings.CssSettings);

            var minifyAttribute = output.Attributes.First(x => x.Name.Equals("minify", StringComparison.InvariantCultureIgnoreCase));
            output.Attributes.Remove(minifyAttribute);

            output.Content.SetHtmlContent(result.Code);
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}
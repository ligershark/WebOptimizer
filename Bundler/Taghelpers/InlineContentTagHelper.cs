using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

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

        /// <summary>
        /// Creates a tag helper for inlining content
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttribute attribute = context.AllAttributes[InlineAttribute];
            string route = attribute.Value.ToString();
            string css = GetFileContent(route);
            output.Content.SetContent(css);
            output.Attributes.Remove(attribute);
        }

        private string GetFileContent(string route)
        {
            if (route.EndsWith(".css"))
                return "div.test{color: blue;}";
            else
                return "function foo(){alert(\"inserted content\");}";
        }
    }
}

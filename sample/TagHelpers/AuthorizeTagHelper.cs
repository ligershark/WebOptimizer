using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BundlerSample
{
    [HtmlTargetElement("*", Attributes = "if-authorized")]
    public class AuthorizeTagHelper : TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        // This makes sure it runs before any other tag helpers
        public override int Order => int.MinValue;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.HttpContext.User.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
            }
            else if (context.AllAttributes.TryGetAttribute("if-authorized", out var attribute))
            {
                output.Attributes.Remove(attribute);
            }
        }
    }
}

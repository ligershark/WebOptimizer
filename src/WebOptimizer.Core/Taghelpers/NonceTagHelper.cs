using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    /// <summary>
    /// A Tag Helper for adding "nonce" attributes to script and style elements.
    /// </summary>
    [HtmlTargetElement("link", Attributes = "href, [rel=stylesheet]")]
    [HtmlTargetElement("link", Attributes = "href, [rel=preload]")]
    [HtmlTargetElement("link", Attributes = "href, [rel=prefetch]")]
    [HtmlTargetElement("script")]
    [HtmlTargetElement("style")]
    public class NonceTagHelper : TagHelper
    {
        private bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonceTagHelper"/> class.
        /// </summary>
        public NonceTagHelper(IOptionsSnapshot<WebOptimizerOptions> options)
        {
            _enabled = options.Value.GenerateNonce == true;
        }

        /// <summary>
        /// When a set of <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" />s are executed, their <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Init(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext)" />'s
        /// are first invoked in the specified <see cref="P:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Order" />; then their
        /// <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext,Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput)" />'s are invoked in the specified
        /// <see cref="P:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Order" />. Lower values are executed first.
        /// </summary>
        public override int Order => int.MaxValue;

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Initializes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" /> with the given <paramref name="context" />. Additions to
        /// <see cref="P:Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext.Items" /> should be done within this method to ensure they're added prior to
        /// executing the children.
        /// </summary>
        public override void Init(TagHelperContext context)
        {
            if (_enabled)
            {
                ViewContext.HttpContext.GetOrCreateNonce();
            }
        }

        /// <summary>
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_enabled || output.Attributes.ContainsName("nonce"))
            {
                return;
            }

            output.Attributes.SetAttribute("nonce", ViewContext.HttpContext.GetOrCreateNonce());
        }
    }

    /// <summary>
    /// Extension methods for the HTTP types
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>
        /// Gets the or create the nonce for this HTTP request/response.
        /// </summary>
        public static string GetOrCreateNonce(this HttpContext context)
        {
            if (!context.Items.TryGetValue("nonce", out object value))
            {
                context.Items["nonce"] = Base64UrlTextEncoder.Encode(Guid.NewGuid().ToByteArray());
            }

            return context.Items["nonce"] as string;
        }
    }
}

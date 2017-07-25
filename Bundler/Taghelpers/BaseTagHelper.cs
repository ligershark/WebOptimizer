using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Bundler.Taghelpers
{
    /// <summary>
    /// A base class for TagHelpers
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" />
    public class BaseTagHelper : TagHelper
    {
        private IHostingEnvironment _env;
        private IMemoryCache _cache;
        private FileVersionProvider _fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
        /// </summary>
        public BaseTagHelper(IHostingEnvironment env, IMemoryCache cache)
        {
            _env = env;
            _cache = cache;
        }

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets the quote character.
        /// </summary>
        protected static string GetQuote(HtmlAttributeValueStyle style)
        {
            switch (style)
            {
                case HtmlAttributeValueStyle.DoubleQuotes:
                    return "\"";
                case HtmlAttributeValueStyle.SingleQuotes:
                    return "'";
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a has of the file.
        /// </summary>
        protected string AddFileVersionToPath(string fileName)
        {
            EnsureVersionFileProvider();
            return _fileProvider.AddFileVersionToPath(fileName);
        }

        /// <summary>
        /// Generates a has of the files in the bundle.
        /// </summary>
        protected string GenerateHash(ITransform transform)
        {
            IEnumerable<string> hashes = transform.SourceFiles.Select(f => AddFileVersionToPath(f));

            return string.Join(string.Empty, hashes).GetHashCode().ToString();
        }

        private void EnsureVersionFileProvider()
        {
            if (_fileProvider == null)
            {
                _fileProvider = new FileVersionProvider(_env.WebRootFileProvider,
                                                        _cache,
                                                        ViewContext.HttpContext.Request.PathBase);
            }
        }
    }
}

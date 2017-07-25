using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using System.Linq;

namespace Bundler.Taghelpers
{
    /// <summary>
    /// A base class for TagHelpers
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" />
    public class BaseTagHelper : TagHelper
    {
        private IHostingEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
        /// </summary>
        public BaseTagHelper(IHostingEnvironment env)
        {
            _env = env;
        }

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
        protected string GenerateHash(string fileName)
        {
            string absolute = Path.Combine(_env.WebRootPath, fileName.TrimStart('/'));
            DateTime lastModified = File.GetLastWriteTime(absolute);

            return lastModified.GetHashCode().ToString();
        }

        /// <summary>
        /// Generates a has of the files in the bundle.
        /// </summary>
        protected string GenerateHash(ITransform transform)
        {
            var hashes = transform.SourceFiles.Select(f => GenerateHash(f));

            return string.Join(string.Empty, hashes).GetHashCode().ToString();
        }
    }
}

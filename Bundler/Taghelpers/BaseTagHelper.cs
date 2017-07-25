using Bundler.Transformers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using System.Linq;

namespace Bundler.Taghelpers
{
    public class BaseTagHelper : TagHelper
    {
        private IHostingEnvironment _env;

        public BaseTagHelper(IHostingEnvironment env)
        {
            _env = env;
        }

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

        protected string GenerateHash(string fileName)
        {
            string absolute = Path.Combine(_env.WebRootPath, fileName.TrimStart('/'));
            DateTime lastModified = File.GetLastWriteTime(absolute);

            return lastModified.GetHashCode().ToString();
        }

        protected string GenerateHash(ITransform transform)
        {
            var hashes = transform.SourceFiles.Select(f => GenerateHash(f));

            return string.Join(string.Empty, hashes).GetHashCode().ToString();
        }
    }
}

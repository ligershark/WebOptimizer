using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bundler.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

namespace Bundler
{
    /// <summary>
    /// Localizes script files by replacing specified tokens with the value from the resource file
    /// </summary>
    internal class Localizer : IProcessor
    {
        private IStringLocalizer _stringProvider;

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context)
        {
            IRequestCultureFeature cf = context.Features.Get<IRequestCultureFeature>();

            if (cf == null)
            {
                throw new InvalidOperationException("No UI culture found.  Did you forget to add UseRequestLocalization?");
            }

            return cf.RequestCulture.UICulture.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// Localizes script files
        /// </summary>
        public Localizer(IStringLocalizer stringProvider)
        {
            _stringProvider = stringProvider;
        }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext config)
        {
            return Task.Run(() =>
            {
                config.Content = Localize(config.Content);
            });
        }

        private string Localize(string document)
        {
            var sb = new StringBuilder();
            const char beginArgChar = '{';
            const char endArgChar = '}';

            int pos = 0;
            int len = document.Length;
            char ch = '\x0';

            while (true)
            {

                while (pos < len)
                {
                    ch = document[pos];
                    pos++;

                    //Is it the beginning of the opening sequence?
                    if (ch == beginArgChar)
                    {
                        //Is it the escape sequence?
                        if (pos < len && document[pos] == beginArgChar)
                        {
                            //Advance to argument hole parameter
                            pos++;
                            break;
                        }
                    }

                    sb.Append(ch);
                }

                //End of the doc string
                if (pos == len) break;

                int beg = pos;
                int paramLen = 0;
                bool argHoleClosed = false;

                while (pos < len)
                {
                    pos++;
                    paramLen++;
                    ch = document[pos];

                    if (ch == endArgChar)
                    {
                        pos++;
                        if (document[pos] == endArgChar)
                        {
                            argHoleClosed = true;
                        }
                        break;
                    }
                }

                if (pos == len) InvalidDocFormat();

                //Advance past the closing char of the argument hole
                pos++;

                string param = document.Substring(beg, paramLen);

                if (!argHoleClosed)
                {
                    InvalidDocFormat(param);
                }

                LocalizedString str = _stringProvider.GetString(param);
                if (str.ResourceNotFound)
                {
                    throw new InvalidOperationException($"No value found for \"{str.Name}\"");
                }
                sb.Append(str.Value);
            }

            return sb.ToString();
        }

        private void InvalidDocFormat()
        {
            throw new InvalidOperationException("Document not correctly formatted");
        }

        private void InvalidDocFormat(string param)
        {
            throw new InvalidOperationException($"{param} argument not correctly terminated (did you forget a '}}'?)");
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static class LocalizerExtensions
    {
        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IEnumerable<IAsset> Localize<T>(this IEnumerable<IAsset> assets)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(AssetManager.Builder);
            var localizer = new Localizer(stringProvider);

            foreach (IAsset asset in assets)
            {
                asset.Processors.Add(localizer);
            }

            return assets;
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IAsset Localize<T>(this IAsset asset)
        {
            IStringLocalizer<T> stringProvider = LocalizationUtilities.GetStringLocalizer<T>(AssetManager.Builder);
            var localizer = new Localizer(stringProvider);

            asset.Processors.Add(localizer);

            return asset;
        }
    }
}

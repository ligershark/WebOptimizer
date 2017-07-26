using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Bundler.Transformers
{
    /// <summary>
    /// Localizes script files by replacing specified tokens with the value from the resource file
    /// </summary>
    public class ScriptLocalizer
    {
        private StringBuilder _sb = new StringBuilder();
        private IStringLocalizer _stringProvider;

        /// <summary>
        /// Localizes script files
        /// </summary>
        private ScriptLocalizer(IStringLocalizer stringProvider)
        {
            _stringProvider = stringProvider;
        }

        private void Append(char c)
        {
            _sb.Append(c);
        }

        private void Append(string s)
        {
            _sb.Append(s);
        }

        /// <summary>
        /// Replaces string keys with values from the resource manager
        /// </summary>
        public static string Localize(string document, IStringLocalizer stringProvider)
        {
            var localizer = new ScriptLocalizer(stringProvider);
            return localizer.Localize(document);
        }


        private string Localize(string document)
        {
            const char beginArgChar = '{';
            const char endArgChar = '}';

            var pos = 0;
            var len = document.Length;
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

                    Append(ch);
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

                var param = document.Substring(beg, paramLen);

                if (!argHoleClosed)
                {
                    InvalidDocFormat(param);
                }

                var str = _stringProvider.GetString(param);
                if (str.ResourceNotFound)
                {
                    throw new InvalidOperationException($"No value found for \"{str.Name}\"");
                }
                Append(str.Value);
            }

            return _sb.ToString();
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
}

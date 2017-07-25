using System;
using System.Collections.Generic;
using System.Text;

namespace Bundler.Transformers
{
    public interface IScriptLocalizer
    {

    }

    public class ScriptLocalizer : IScriptLocalizer
    {
        private StringBuilder _sb = new StringBuilder();
        private ILocalizedStringProvider _stringProvider;

        public ScriptLocalizer(ILocalizedStringProvider stringProvider)
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

        public string Format(string doc)
        {
            const char beginArgChar = '{';
            const char endArgChar = '}';

            var pos = 0;
            var len = doc.Length;
            char ch = '\x0';

            while (true)
            {

                while (pos < len)
                {
                    ch = doc[pos];
                    pos++;

                    //Is it the beginning of the opening sequence?
                    if (ch == beginArgChar)
                    {
                        //Is it the escape sequence?
                        if (pos < len && doc[pos] == beginArgChar)
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
                    ch = doc[pos];

                    if (ch == endArgChar)
                    {
                        pos++;
                        if (doc[pos] == endArgChar)
                        {
                            argHoleClosed = true;
                        }
                        break;
                    }
                }

                if (pos == len) InvalidDocFormat();

                //Advance past the closing char of the argument hole
                pos++;

                var param = doc.Substring(beg, paramLen);

                if (!argHoleClosed)
                {
                    InvalidDocFormat(param);
                }

                Append(provider.Get(param));
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

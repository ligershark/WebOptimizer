using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebOptimizer
{
    /// <summary>
    /// Extension methods for making it easier to work with streams.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Converts the byte array to a string
        /// </summary>
        public static string AsString(this byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts a string into a byte array.
        /// </summary>
        public static byte[] AsByteArray(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Converts a stream to a byte array
        /// </summary>
        public static async Task<byte[]> AsBytesAsync(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var ms = new MemoryStream())
            {
                stream.Position = 0;
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}

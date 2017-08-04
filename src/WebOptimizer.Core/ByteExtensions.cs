using System.IO;
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
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts a string into a byte array.
        /// </summary>
        public static byte[] AsByteArray(this string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// Converts a stream to a byte array
        /// </summary>
        public static async Task<byte[]> AsBytesAsync(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}

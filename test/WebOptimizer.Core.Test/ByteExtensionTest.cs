using System.IO;
using System.Text;
using Xunit;

namespace WebOptimizer.Core.Test
{
    public class ByteExtensionTest
    {
        [Theory2]
        [InlineData("ascii")]
        [InlineData("æøå")]
        [InlineData("аз буки веди")]
        public void AsString_Success(string input)
        {
            string output = Encoding.UTF8.GetBytes(input).AsString();

            Assert.Equal(input, output);
        }

        [Theory2]
        [InlineData("ascii", 5)]
        [InlineData("æøå", 6)]
        [InlineData("аз буки веди", 22)]
        public void AsByteArray_Success(string input, int length)
        {
            var output = input.AsByteArray();

            Assert.Equal(length, output.Length);
        }

        [Theory2]
        [InlineData("ascii")]
        [InlineData("æøå")]
        [InlineData("аз буки веди")]
        public void AsBytesAsync_Success(string input)
        {
            var bytes = input.AsByteArray();

            using (var ms = new MemoryStream(bytes))
            using (var reader = new StreamReader(ms))
            {
                string output = reader.ReadToEnd();
                Assert.Equal(input, output);
            }
        }
    }
}

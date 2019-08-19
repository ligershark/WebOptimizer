using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace WebOptimizer.Core.Test
{
    public class AssetResponseStoreTest
    {
        [Fact2("_"," ")]
        public void Should_Serialize_Deserialize_AssetResponse_Without_Headers()
        {
            string cacheKey = "cachekey";
            string body = "*{color:red}";
            AssetResponse arBase = new AssetResponse(body.AsByteArray(), cacheKey);
            var logger = new Mock<ILogger<AssetResponseStore>>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            string path = Path.Combine(Environment.CurrentDirectory, "WebOptimizerTest");
            string filePath = Path.Combine(path, "bucket__cachekey.cache");

            var woo = new Mock<IConfigureOptions<WebOptimizerOptions>>();
            woo.Setup(o => o.Configure(It.IsAny<WebOptimizerOptions>()))
                .Callback<WebOptimizerOptions>(o => o.CacheDirectory = path);
            AssetResponse ar = new AssetResponseStore(logger.Object,env.Object,woo.Object).ParseJson(JsonSerializer.Serialize(arBase));
            Assert.Equal(cacheKey, ar.CacheKey);
            Assert.Equal(body.AsByteArray(), ar.Body );
            Assert.Equal(body, Encoding.ASCII.GetString(ar.Body));
            Assert.Equal(arBase.Headers.Count, ar.Headers.Count);
        }
        [Fact2("_"," ")]
        public void Should_Serialize_Deserialize_AssetResponse_With_Headers()
        {
            string cacheKey = "cachekey";
            string body = "*{color:red}";
            AssetResponse arBase = new AssetResponse(body.AsByteArray(), cacheKey);
            string key1 = "api-version";
            string value1 = "3.0";
            arBase.Headers.Add(key1, value1);
            string key2 = "content-type";
            string value2 = "application/json";
            arBase.Headers.Add(key2, value2);
            //string json = "{\"Headers\":{\"api-version\":\"3.0\",\"content-type\":\"application/json\"},\"Body\":\"Kntjb2xvcjpyZWR9\",\"CacheKey\":\"cachekey\"}";
            var logger = new Mock<ILogger<AssetResponseStore>>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            string path = Path.Combine(Environment.CurrentDirectory, "WebOptimizerTest");
            string filePath = Path.Combine(path, "bucket__cachekey.cache");

            var woo = new Mock<IConfigureOptions<WebOptimizerOptions>>();
            woo.Setup(o => o.Configure(It.IsAny<WebOptimizerOptions>()))
                .Callback<WebOptimizerOptions>(o => o.CacheDirectory = path);

            AssetResponse ar = new AssetResponseStore(logger.Object,env.Object,woo.Object).ParseJson(JsonSerializer.Serialize(arBase));

            //using (JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(arBase)))
            //{
            //    var ck = document.RootElement.GetProperty("CacheKey").GetString();
            //    var b = document.RootElement.GetProperty("Body").GetString();
            //    var bytes = JsonSerializer.Deserialize<byte[]>("\"" + b + "\"");
            //    ar = new AssetResponse(bytes,ck);
            //    var headersString = document.RootElement.GetProperty("Headers").GetRawText();
            //    if (!string.IsNullOrEmpty(headersString))
            //    {
            //        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersString);
            //        foreach (var d in headers)
            //        {
            //            ar.Headers.Add(d.Key, d.Value);
            //        }
            //    }
            //}

            Assert.Equal(cacheKey, ar.CacheKey);
            Assert.Equal(body.AsByteArray(), ar.Body );
            Assert.Equal(body, Encoding.ASCII.GetString(ar.Body));
            Assert.Equal(arBase.Headers.Count, ar.Headers.Count);
            Assert.True(ar.Headers.ContainsKey(key1));
            Assert.Equal(arBase.Headers[key1], ar.Headers[key1]);
            Assert.True(ar.Headers.ContainsKey(key2));
            Assert.Equal(arBase.Headers[key2], ar.Headers[key2]);
        }
        [Fact2]
        public async Task RoundTrip_Success()
        {
            var logger = new Mock<ILogger<AssetResponseStore>>();
            var env = new Mock<IWebHostEnvironment>().SetupAllProperties();
            env.Setup(e => e.ContentRootPath).Returns(Environment.CurrentDirectory);
            string path = Path.Combine(Environment.CurrentDirectory, "WebOptimizerTest");
            string filePath = Path.Combine(path, "bucket__cachekey.cache");

            var woo = new Mock<IConfigureOptions<WebOptimizerOptions>>();
            woo.Setup(o => o.Configure(It.IsAny<WebOptimizerOptions>()))
                .Callback<WebOptimizerOptions>(o => o.CacheDirectory = path);

            byte[] body = "*{color:red}".AsByteArray();
            var before = new AssetResponse(body, "cachekey");
            //var outJson1 = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<AssetResponse>(before);
            //var outJson2 = System.Text.Json.JsonSerializer.Serialize<AssetResponse>(before);
            IAssetResponseStore ars = new AssetResponseStore(logger.Object, env.Object, woo.Object);

            await ars.AddAsync("bucket", "cachekey", before).ConfigureAwait(false);
            Assert.True(File.Exists(filePath));

            Assert.True(ars.TryGet("bucket", "cachekey", out var after));
            Assert.Equal(before.CacheKey, after.CacheKey);
            Assert.Equal(before.Body, after.Body);

            await ars.RemoveAsync("bucket", "cachekey").ConfigureAwait(false);

            Assert.False(File.Exists(filePath));
        }
    }
}

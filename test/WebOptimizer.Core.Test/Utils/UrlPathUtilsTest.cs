using System;
using System.Collections.Generic;
using System.IO;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Core.Test.Utils
{
    public class UrlPathUtilsTest
    {
        [Theory2]
        [InlineData("/foo", "/foo")]
        [InlineData("foo", "foo")]
        [InlineData("/foo/bar", "/foo/bar")]
        [InlineData("foo/bar", "foo/bar")]
        [InlineData("/foo/bar/baz", "/foo/bar/baz")]
        [InlineData("foo/bar/baz", "foo/bar/baz")]
        [InlineData("/foo/bar/", "/foo/bar/")]
        [InlineData("foo/bar/", "foo/bar/")]
        [InlineData("/foo/./bar", "/foo/bar")]
        [InlineData("foo/./bar", "foo/bar")]
        [InlineData("/foo/../bar", "/bar")]
        [InlineData("foo/../bar", "bar")]
        [InlineData("/foo/../bar/../baz", "/baz")]
        [InlineData("foo/../bar/../baz", "baz")]
        [InlineData("foo/../../bar", "../bar")]
        [InlineData("../foo/bar", "../foo/bar")]
        [InlineData("../foo/../bar", "../bar")]
        [InlineData("../../foo/../bar", "../../bar")]
        [InlineData(".././../foo/./.././bar", "../../bar")]
        [InlineData("./foo", "foo")]
        [InlineData("foo/.", "foo")]
        [InlineData("foo/./", "foo/")]
        [InlineData("/./foo", "/foo")]
        public void Normalize_Success(string url, string normalizedUrl)
        {
            string result = UrlPathUtils.Normalize(url);
            Assert.Equal(normalizedUrl, result);
        }

        [Theory2]
        [InlineData("/foo/../../bar")]
        [InlineData("/foo/.././../bar")]
        public void Normalize_Throw(string url)
        {
            var ex = Assert.Throws<ArgumentException>(() => UrlPathUtils.Normalize(url));
        }

        [Theory2]
        [InlineData("/foo", true, "/foo")]
        [InlineData("foo", true, "foo")]
        [InlineData("/foo/bar", true, "/foo/bar")]
        [InlineData("foo/bar", true, "foo/bar")]
        [InlineData("/foo/bar/baz", true, "/foo/bar/baz")]
        [InlineData("foo/bar/baz", true, "foo/bar/baz")]
        [InlineData("/foo/bar/", true, "/foo/bar/")]
        [InlineData("foo/bar/", true, "foo/bar/")]
        [InlineData("/foo/./bar", true, "/foo/bar")]
        [InlineData("foo/./bar", true, "foo/bar")]
        [InlineData("/foo/../bar", true, "/bar")]
        [InlineData("foo/../bar", true, "bar")]
        [InlineData("/foo/../bar/../baz", true, "/baz")]
        [InlineData("foo/../bar/../baz", true, "baz")]
        [InlineData("foo/../../bar", true, "../bar")]
        [InlineData("../foo/bar", true, "../foo/bar")]
        [InlineData("../foo/../bar", true, "../bar")]
        [InlineData("../../foo/../bar", true, "../../bar")]
        [InlineData(".././../foo/./.././bar", true, "../../bar")]
        [InlineData("./foo", true, "foo")]
        [InlineData("foo/.", true, "foo")]
        [InlineData("foo/./", true, "foo/")]
        [InlineData("/./foo", true, "/foo")]
        [InlineData("/foo/../../bar", false, default(string))]
        [InlineData("/foo/.././../bar", false, default(string))]
        public void TryNormalize_Success(string url, bool isSuccess, string normalizedUrl)
        {
            bool result = UrlPathUtils.TryNormalize(url, out string resultUrl);

            Assert.Equal(isSuccess, result);

            if (isSuccess)
                Assert.Equal(normalizedUrl, resultUrl);
        }


        [Theory2]
        [InlineData("/", true)]
        [InlineData("/foo", true)]
        [InlineData("/foo/bar", true)]
        [InlineData("", false)]
        [InlineData("foo", false)]
        [InlineData("foo/bar", false)]
        public void IsAbsolutePath_Success(string url, bool isAbsoluteUrl)
        {
            bool result = UrlPathUtils.IsAbsolutePath(url);
            Assert.Equal(isAbsoluteUrl, result);
        }

        [Theory2]
        [InlineData("/foo", "bar", "/foo/bar")]
        [InlineData("/foo", "/bar", "/bar")]
        [InlineData("/foo/", "bar", "/foo/bar")]
        [InlineData("/foo/", "/bar", "/bar")]
        [InlineData("/foo", "bar/", "/foo/bar/")]
        [InlineData("/foo", "/bar/", "/bar/")]
        [InlineData("/foo", "./bar", "/foo/bar")]
        [InlineData("/foo", "../bar", "/bar")]
        [InlineData("/foo/bar", "../", "/foo/")]
        [InlineData("/foo/hello/world", "../../bar", "/foo/bar")]
        public void MakeAbsolute_Success(string basePath, string path, string absolutePath)
        {
            string result = UrlPathUtils.MakeAbsolute(basePath, path);
            Assert.Equal(absolutePath, result);
        }

        [Theory2]
        [InlineData("foo", "bar")]
        [InlineData("foo/", "bar/")]
        [InlineData("./", "bar")]
        [InlineData("../", "bar")]
        [InlineData("/foo", "../../bar")]
        [InlineData("foo", "./bar")]
        public void MakeAbsolute_Throw(string basePath, string path)
        {
            var ex = Assert.Throws<ArgumentException>(() => UrlPathUtils.MakeAbsolute(basePath, path));
        }

        [Theory2]
        [InlineData("", "")]
        [InlineData("/", "/")]
        [InlineData("/foo", "/")]
        [InlineData("/foo/", "/foo/")]
        [InlineData("/foo/bar", "/foo/")]
        [InlineData("/foo/bar/", "/foo/bar/")]
        [InlineData("foo", "")]
        [InlineData("foo/", "foo/")]
        [InlineData("foo/bar", "foo/")]
        [InlineData("foo/bar/", "foo/bar/")]
        public void GetDirectory_Success(string url, string directory)
        {
            string result = UrlPathUtils.GetDirectory(url);
            Assert.Equal(directory, result);
        }

        [Theory2]
        [InlineData("/foo", "foo")]
        [InlineData("/foo.css", "foo.css")]
        [InlineData("/foo/bar", "bar")]
        [InlineData("/foo/bar.css", "bar.css")]
        public void GetFileName_Success(string path, string fileName)
        {
            string result = UrlPathUtils.GetFileName(path);
            Assert.Equal(fileName, result);
        }

        [Theory2]
        [InlineData("/")]
        [InlineData("/foo.css/")]
        [InlineData("/foo/")]
        [InlineData("/foo/bar.css/")]
        [InlineData("/foo/bar/")]
        public void GetFileName_Throw(string path)
        {
            var ex = Assert.Throws<ArgumentException>(() => UrlPathUtils.GetFileName(path));
        }

        [Theory2]
        [InlineData("/", "css/site.css", "/img/test.png", "/img/test.png")]
        [InlineData("/", "css/site.css", "img/test.png", "/css/img/test.png")]
        [InlineData("/", "css/site.css", "../img/test.png", "/img/test.png")]
        [InlineData("/foo", "css/site.css", "/img/test.png", "/img/test.png")]
        [InlineData("/foo", "css/site.css", "img/test.png", "/foo/css/img/test.png")]
        [InlineData("/foo", "css/site.css", "../img/test.png", "/foo/img/test.png")]
        [InlineData("/foo/", "css/site.css", "../img/test.png", "/foo/img/test.png")]
        [InlineData("/", "site.css", "img/test.png", "/img/test.png")]
        [InlineData("/foo", "site.css", "img/test.png", "/foo/img/test.png")]
        [InlineData("/foo", "css/site.css", "../../img/test.png", "/img/test.png")]
        public void MakeAbsolutePathFromInclude_Success(string appPath, string contentPath, string includePath, string absolutePath)
        {
            string result = UrlPathUtils.MakeAbsolutePathFromInclude(appPath, contentPath, includePath);
            Assert.Equal(absolutePath, result);
        }

        [Theory2]
        [InlineData("/", "css/site.css", "/img/test.png", true, "/img/test.png")]
        [InlineData("/", "css/site.css", "img/test.png", true, "/css/img/test.png")]
        [InlineData("/", "css/site.css", "../img/test.png", true, "/img/test.png")]
        [InlineData("/foo", "css/site.css", "/img/test.png", true, "/img/test.png")]
        [InlineData("/foo", "css/site.css", "img/test.png", true, "/foo/css/img/test.png")]
        [InlineData("/foo", "css/site.css", "../img/test.png", true, "/foo/img/test.png")]
        [InlineData("/foo/", "css/site.css", "../img/test.png", true, "/foo/img/test.png")]
        [InlineData("/", "site.css", "img/test.png", true, "/img/test.png")]
        [InlineData("/foo", "site.css", "img/test.png", true, "/foo/img/test.png")]
        [InlineData("/foo", "css/site.css", "../../img/test.png", true, "/img/test.png")]
        [InlineData("/", "css/site.css", "../../img/test.png", false, default(string))]
        [InlineData("/", "site.css", "../img/test.png", false, default(string))]
        public void TryMakeAbsolutePathFromInclude_Success(string appPath, string contentPath, string includePath, bool isSuccess, string absolutePath)
        {
            bool result = UrlPathUtils.TryMakeAbsolutePathFromInclude(appPath, contentPath, includePath, out string pathResult);

            Assert.Equal(isSuccess, result);

            if (isSuccess)
                Assert.Equal(absolutePath, pathResult);
        }
    }
}

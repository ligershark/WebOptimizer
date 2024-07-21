using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer.Core.Test.Mocks
{
    internal class MockFileInfo : IFileInfo
    {
        private readonly byte[] _data;

        public MockFileInfo(string name, DateTimeOffset lastModified, byte[] data)
        {
            _data = data;
            Name = name;
            LastModified = lastModified;
        }

        public Stream CreateReadStream()
        {
            return new MemoryStream(_data, false);
        }

        public bool Exists => true;
        public bool IsDirectory => false;
        public DateTimeOffset LastModified { get; }
        public long Length => _data.Length;
        public string Name { get; }
        public string PhysicalPath => null;
    }

}

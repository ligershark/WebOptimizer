using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer.Core.Test.Mocks;

internal class MockFileInfo(string name, DateTimeOffset lastModified, byte[] data) : IFileInfo
{
    public bool Exists => true;

    public bool IsDirectory => false;

    public DateTimeOffset LastModified { get; } = lastModified;

    public long Length => data.Length;

    public string Name { get; } = name;

    public string? PhysicalPath => null;

    public Stream CreateReadStream() => new MemoryStream(data, false);
}

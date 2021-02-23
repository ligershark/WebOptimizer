using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace WebOptimizer
{
    public class FileProviderOptions
    {
        public PathString RequestPath { get; set; }
        public IFileProvider FileProvider { get; set; }
    }

    internal class CompositeFileProviderExtended : IFileProvider
    {
        private readonly IFileProvider _webRootFileProvider;
        private readonly FileProviderOptions[] _fileProviderOptions;

        public CompositeFileProviderExtended(IFileProvider webRootFileProvider, FileProviderOptions[] fileProviderOptions)
        {
            _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));
            _fileProviderOptions = fileProviderOptions;
        }

        public IDirectoryContents GetDirectoryContents(string subPath)
        {
            IFileProvider provider = GetFileProvider(subPath, out string outPath);

            return provider.GetDirectoryContents(outPath);
        }

        public IFileInfo GetFileInfo(string subPath)
        {
            IFileProvider provider = GetFileProvider(subPath, out string outPath);

            return provider.GetFileInfo(outPath);
        }

        public IChangeToken Watch(string filter)
        {
            IFileProvider provider = GetFileProvider(filter, out string outPath);

            return provider.Watch(outPath);
        }

        internal IFileProvider GetFileProvider(string path, out string outPath)
        {
            outPath = path;

            FileProviderOptions[] fileProviders = _fileProviderOptions;
            if (fileProviders != null)
            {
                for (var index = 0; index < fileProviders.Length; index++)
                {
                    var item = fileProviders[index];

                    if (path.StartsWith(item.RequestPath, StringComparison.OrdinalIgnoreCase))
                    {
                        outPath = path.Substring(item.RequestPath.Value.Length, path.Length - item.RequestPath.Value.Length);

                        return item.FileProvider;
                    }
                }
            }

            return _webRootFileProvider;
        }
    }
}

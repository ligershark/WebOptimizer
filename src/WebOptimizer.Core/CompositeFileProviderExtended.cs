using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace WebOptimizer
{
    internal class CompositeFileProviderExtended : IFileProvider
    {
        private readonly IFileProvider _webRootFileProvider;
        private readonly StaticFileOptions[] _staticFileOptions;

        public CompositeFileProviderExtended(IFileProvider webRootFileProvider, StaticFileOptions[] staticFileOptions)
        {
            _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));
            _staticFileOptions = staticFileOptions;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            string outpath;
            var provider = GetFileProvider(subpath, out outpath);

            return provider.GetDirectoryContents(outpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            string outpath;
            var provider = GetFileProvider(subpath, out outpath);

            return provider.GetFileInfo(outpath);
        }

        public IChangeToken Watch(string filter)
        {
            string outpath;
            var provider = GetFileProvider(filter, out outpath);

            return provider.Watch(outpath);
        }

        internal IFileProvider GetFileProvider(string path, out string outpath)
        {
            outpath = path;

            var fileProviders = _staticFileOptions;
            if (fileProviders != null)
            {
                for (var index = 0; index < fileProviders.Length; index++)
                {
                    var item = fileProviders[index];

                    if (path.StartsWith(item.RequestPath, StringComparison.Ordinal))
                    {
                        outpath = path.Substring(item.RequestPath.Value.Length, path.Length - item.RequestPath.Value.Length);

                        return item.FileProvider;
                    }
                }
            }

            return _webRootFileProvider;
        }
    }
}

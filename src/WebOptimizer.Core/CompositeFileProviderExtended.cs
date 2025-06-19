using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace WebOptimizer;

/// <summary>
/// Composite file provider that allows serving static files from multiple sources.
/// Implements the <see cref="IFileProvider" />
/// </summary>
/// <seealso cref="IFileProvider" />
internal class CompositeFileProviderExtended(IFileProvider webRootFileProvider, FileProviderOptions[] fileProviderOptions) : IFileProvider
{
    private readonly IFileProvider _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));

    public IDirectoryContents GetDirectoryContents(string subPath)
    {
        var provider = GetFileProvider(subPath, out string outPath);

        return provider.GetDirectoryContents(outPath);
    }

    public IFileInfo GetFileInfo(string subPath)
    {
        var provider = GetFileProvider(subPath, out string outPath);

        return provider.GetFileInfo(outPath);
    }

    public IChangeToken Watch(string filter)
    {
        var provider = GetFileProvider(filter, out string outPath);

        return provider.Watch(outPath);
    }

    internal IFileProvider GetFileProvider(string path, out string outPath)
    {
        outPath = path;

        var fileProviders = fileProviderOptions;
        if (fileProviders is not null)
        {
            foreach (var item in fileProviders)
            {
                if (path.StartsWith(item.RequestPath, StringComparison.OrdinalIgnoreCase))
                {
                    outPath = path[(item.RequestPath.Value?.Length ?? 0)..];

                    return item.FileProvider;
                }
            }
        }

        return _webRootFileProvider;
    }
}

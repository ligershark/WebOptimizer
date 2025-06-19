using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer;

/// <summary>
/// Represents options for a file provider that can be used to serve static files from multiple sources.
/// </summary>
public class FileProviderOptions
{
    /// <summary>
    /// Gets or sets the file provider.
    /// </summary>
    /// <value>The file provider.</value>
    public IFileProvider FileProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    /// <value>The request path.</value>
    public PathString RequestPath { get; set; } = default;
}

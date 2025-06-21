using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer.Core.Test.Mocks;

/// <summary>
/// Mock implementation of <see cref="IWebHostEnvironment"/> for testing purposes.
/// </summary>
/// <seealso cref="IWebHostEnvironment" />
public class HostingEnvironment : IWebHostEnvironment
{
    /// <summary>
    /// Gets or sets the name of the application. This property is automatically set by the host to the assembly containing
    /// the application entry point.
    /// </summary>
    /// <value>The name of the application.</value>
    public string ApplicationName { get; set; } = default!;

    /// <summary>
    /// Gets or sets an <see cref="T:Microsoft.Extensions.FileProviders.IFileProvider" /> pointing at <see cref="P:Microsoft.Extensions.Hosting.IHostEnvironment.ContentRootPath" />.
    /// </summary>
    /// <value>The content root file provider.</value>
    public IFileProvider ContentRootFileProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the absolute path to the directory that contains the application content files.
    /// </summary>
    /// <value>The content root path.</value>
    public string ContentRootPath { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the environment. The host automatically sets this property to the value of the
    /// "environment" key as specified in configuration.
    /// </summary>
    /// <value>The name of the environment.</value>
    public string EnvironmentName { get; set; } = default!;

    /// <summary>
    /// Gets or sets an <see cref="T:Microsoft.Extensions.FileProviders.IFileProvider" /> pointing at <see cref="P:Microsoft.AspNetCore.Hosting.IWebHostEnvironment.WebRootPath" />.
    /// This defaults to referencing files from the 'wwwroot' subfolder.
    /// </summary>
    /// <value>The web root file provider.</value>
    public IFileProvider WebRootFileProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the absolute path to the directory that contains the web-servable application content files.
    /// This defaults to the 'wwwroot' subfolder.
    /// </summary>
    /// <value>The web root path.</value>
    public string WebRootPath { get; set; } = default!;
}

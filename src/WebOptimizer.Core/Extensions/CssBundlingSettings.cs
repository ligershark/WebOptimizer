using NUglify.Css;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Settings for CSS bundling and minification.
/// </summary>
public class CssBundlingSettings
{
    /// <summary>
    /// Gets the CSS settings.
    /// </summary>
    /// <value>The CSS settings.</value>
    public CssSettings CssSettings { get; } = new CssSettings();

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CssBundlingSettings"/> is minify.
    /// </summary>
    /// <value><c>true</c> if minify; otherwise, <c>false</c>.</value>
    public bool Minify { get; set; } = true;

    /// <summary>
    /// Gets or sets the enforce file extensions.
    /// </summary>
    /// <value>The enforce file extensions.</value>
    public string[] EnforceFileExtensions { get; set; } = [".css"];

    /// <summary>
    /// Gets or sets a value indicating whether [adjust relative paths].
    /// </summary>
    /// <value><c>true</c> if [adjust relative paths]; otherwise, <c>false</c>.</value>
    public bool AdjustRelativePaths { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CssBundlingSettings"/> is concatenate.
    /// </summary>
    /// <value><c>true</c> if concatenate; otherwise, <c>false</c>.</value>
    public bool Concatenate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether [fingerprint urls].
    /// </summary>
    /// <value><c>true</c> if [fingerprint urls]; otherwise, <c>false</c>.</value>
    public bool FingerprintUrls { get; set; } = true;
}
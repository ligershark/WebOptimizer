using NUglify.JavaScript;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Settings for code bundling operations.
/// </summary>
public class CodeBundlingSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether [adjust relative paths].
    /// </summary>
    /// <value><c>true</c> if [adjust relative paths]; otherwise, <c>false</c>.</value>
    public bool AdjustRelativePaths { get; set; } = true;

    /// <summary>
    /// Gets the code settings.
    /// </summary>
    /// <value>The code settings.</value>
    public CodeSettings CodeSettings { get; } = new CodeSettings();

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CodeBundlingSettings"/> is concatenate.
    /// </summary>
    /// <value><c>true</c> if concatenate; otherwise, <c>false</c>.</value>
    public bool Concatenate { get; set; } = true;

    /// <summary>
    /// Gets or sets the enforce file extensions.
    /// </summary>
    /// <value>The enforce file extensions.</value>
    public string[] EnforceFileExtensions { get; set; } = [".js"];

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CodeBundlingSettings"/> is minify.
    /// </summary>
    /// <value><c>true</c> if minify; otherwise, <c>false</c>.</value>
    public bool Minify { get; set; } = true;
}
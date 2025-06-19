namespace WebOptimizer.Processors;

/// <summary>
/// Settings for JavaScript Bundles
/// </summary>
/// <param name="nuglifyCodeSettings">The nuglify code settings.</param>
public class JsSettings(NUglify.JavaScript.CodeSettings? nuglifyCodeSettings = null)
{
    /// <summary>
    /// NUglify is the underlying minifier for WebOptimizer. It's derived from Microsoft's AjaxMin.
    /// </summary>
    public NUglify.JavaScript.CodeSettings CodeSettings { get; set; } = nuglifyCodeSettings ?? new NUglify.JavaScript.CodeSettings();

    /// <summary>
    /// Defaults to false. Whether to generate source maps, allowing bundled code to be debugged
    /// using original source in places like Chrome Dev Tools. Respects .SymbolsMap on base class,
    /// CodeSettings; this setting is ignored (treated as false) if caller sets .SymbolsMap
    /// </summary>
    public bool GenerateSourceMap { get; set; }

    /// <summary>
    /// Set by the framework if GenerateSourceMap is true Helps a given Bundle Asset identify its
    /// SourceMap Asset to write to.
    /// </summary>
    public IAsset? PipelineSourceMap { get; set; }
}

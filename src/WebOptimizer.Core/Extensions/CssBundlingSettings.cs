using NUglify.Css;

namespace Microsoft.Extensions.DependencyInjection
{
    public class CssBundlingSettings
    {
        public CssSettings CssSettings { get; } = new CssSettings();
        public bool Minify { get; set; } = true;
        public string[] EnforceFileExtensions { get; set; } = {".css"};
        public bool AdjustRelativePaths { get; set; } = true;
        public bool Concatenate { get; set; } = true;
        public bool FingerprintUrls { get; set; } = true;
    }
}
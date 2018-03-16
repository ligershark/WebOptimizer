using NUglify.JavaScript;

namespace Microsoft.Extensions.DependencyInjection
{
    public class CodeBundlingSettings
    {
        public CodeSettings CodeSettings { get; } = new CodeSettings();
        public bool Minify { get; set; } = true;
        public string[] EnforceFileExtensions { get; set; } = {".js"};
        public bool AdjustRelativePaths { get; set; } = true;
        public bool Concatenate { get; set; } = true;
    }
}
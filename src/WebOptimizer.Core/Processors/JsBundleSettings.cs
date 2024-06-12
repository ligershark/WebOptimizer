using System;
using System.Collections.Generic;
using System.Text;


namespace WebOptimizer.Processors
{
    public class JsSettings
    {
        /// <summary>
        /// Defaults to false.
        /// Whether to generate source maps, allowing bundled code to be debugged using original source in places like Chrome Dev Tools.
        /// Respects .SymbolsMap on base class, CodeSettings; this setting is ignored (treated as false) if caller sets .SymbolsMap
        /// </summary>
        public bool GenerateSourceMap { get; set; }

        /// <summary>
        /// Set by the framework if GenerateSourceMap is true
        /// Helps a given Bundle Asset identify its SourceMap Asset to write to.
        /// </summary>
        public IAsset PipelineSourceMap { get; set; }

        /// <summary>
        /// NUglify is the underlying minifier for WebOptimizer.
        /// It's derived from Microsoft's AjaxMin.
        /// </summary>
        public NUglify.JavaScript.CodeSettings CodeSettings { get; set; }



        public JsSettings()
        {
            CodeSettings = new NUglify.JavaScript.CodeSettings();
        }
        public JsSettings(NUglify.JavaScript.CodeSettings nuglifyCodeSettings)
        {
            CodeSettings = nuglifyCodeSettings;
        }
    }
}

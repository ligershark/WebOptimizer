using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using WebOptimizer.TagHelpersDynamic;

namespace WebOptimizer.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class TagHelperExtensions
    {
        /// <summary>
        /// Process output of the tag helper
        /// </summary>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <param name="pipeline">The web optimization pipeline</param>
        /// <param name="actionContext">Context object for execution of action</param>
        /// <param name="options">Options for the Web Optimizer</param>
        /// <param name="src">Source path value</param>
        /// <param name="bundleKey">A key of associated bundle</param>
        /// <param name="destBundleKey">The bundle key; Pass to generate the bundle tag.</param>
        /// <returns>true - If processing is successful, otherwise - false</returns>
        public static bool HandleJsBundle(this TagHelperOutput output,
            IAssetPipeline pipeline,
            ActionContext actionContext,
            WebOptimizerOptions options,
            string src,
            string bundleKey,
            string destBundleKey)
        {
            return Helpers
                    .HandleBundle(
                        Helpers.CreateJsAsset,
                        pipeline,
                        output,
                        actionContext,
                        options,
                        "src", src, bundleKey, destBundleKey);
        }

        /// <summary>
        /// Process output of the tag helper
        /// </summary>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <param name="pipeline">The web optimization pipeline</param>
        /// <param name="actionContext">Context object for execution of action</param>
        /// <param name="options">Options for the Web Optimizer</param>
        /// <param name="href">Source path value</param>
        /// <param name="bundleKey">A key of associated bundle</param>
        /// <param name="destBundleKey">The bundle key; Pass to generate the bundle tag.</param>
        /// <returns>true - If processing is successful, otherwise - false</returns>
        public static bool HandleCssBundle(this TagHelperOutput output,
            IAssetPipeline pipeline,
            ActionContext actionContext,
            WebOptimizerOptions options,
            string href,
            string bundleKey,
            string destBundleKey)
        {
            return Helpers
                    .HandleBundle(
                        Helpers.CreateCssAsset,
                        pipeline,
                        output,
                        actionContext,
                        options,
                        "href", href, bundleKey, destBundleKey);
        }
    }
}

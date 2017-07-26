using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Bundler.Utilities
{
    internal class LocalizationUtilities
    {
        /// <summary>
        /// Gets the string localizer of type 'T' from the app builder
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IStringLocalizer<T> GetStringLocalizer<T>(IApplicationBuilder app)
        {
            try
            {
                IStringLocalizer<T> stringProvider = app.ApplicationServices.GetRequiredService<IStringLocalizer<T>>();
                return stringProvider;
            }
            catch (InvalidOperationException e)
            {
                if (e.HResult == -2146233079)
                {
                    throw new InvalidOperationException("No IStringLocalizer could be found.  Did you forget to register localization middleware in ConfigureServices?");
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the UI culture of the incoming request
        /// </summary>
        /// <param name="config"></param>
        /// <returns>UI Culture of the request</returns>
        public static CultureInfo GetRequestUICulture(BundlerContext config)
        {
            IRequestCultureFeature cf = config.HttpContext.Features.Get<IRequestCultureFeature>();

            if (cf == null)
            {
                throw new InvalidOperationException("No UI culture found.  Did you forget to add UseRequestLocalization?");
            }

            return cf.RequestCulture.UICulture;
        }
    }
}

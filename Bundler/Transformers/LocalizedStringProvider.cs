using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;

namespace Bundler.Transformers
{
    public interface ILocalizedStringProvider
    {
        string GetString(string key);

        string GetString(string key, CultureInfo culture);
    }

    public class LocalizedStringProvider : ILocalizedStringProvider
    {
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public LocalizedStringProvider(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            _currentCulture = CultureInfo.CurrentCulture;
        }

        public LocalizedStringProvider(ResourceManager resourceManager, CultureInfo culture)
        {
            _resourceManager = resourceManager;
            _currentCulture = culture;
        }

        public string GetString(string key)
        {
            return _resourceManager.GetString(key);
        }

        public string GetString(string key, CultureInfo culture)
        {
            return _resourceManager.GetString(key, culture);
        }
    }
}

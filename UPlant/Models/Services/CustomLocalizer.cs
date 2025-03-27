using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UPlant.Models.Services
{
    public class CustomLocalizer : IStringLocalizer
    {
        private readonly IStringLocalizer localizer;
        public CustomLocalizer(IStringLocalizerFactory localizerFactory)
        {
            this.localizer = localizerFactory.Create("Resource", "UPlant");
        }
        ////Il nostro CustomLocalizer è di fatto un wrapper attorno all'IStringLocalizer creato dalla factory
        ///
        public LocalizedString this[string name] => localizer[name];
        public LocalizedString this[string name, params object[] arguments] => localizer[name, arguments];

        public IEnumerable<LocalizedString>
          GetAllStrings(bool includeParentCultures) => localizer.GetAllStrings(includeParentCultures);


        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LocalizationService
    {
        private readonly IStringLocalizer _localizer;

        public LocalizationService(IStringLocalizerFactory factory)
        {
            var type = typeof(UPlant.Resources.Resource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("Resource", assemblyName.Name);
        }

        public LocalizedString GetLocalizedHtmlString(string key)
        {
            return _localizer[key];
        }
    }
}
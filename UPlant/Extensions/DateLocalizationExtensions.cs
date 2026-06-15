using System;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UPlant.Extensions
{
    public static class DateLocalizationExtensions
    {
        public static string LocalizedDate(this IHtmlHelper html, DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("d", CultureInfo.CurrentCulture) : string.Empty;
        }

        public static string LocalizedDate(this IHtmlHelper html, DateTime value)
        {
            return value.ToString("d", CultureInfo.CurrentCulture);
        }

        public static string LocalizedDateTime(this IHtmlHelper html, DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("g", CultureInfo.CurrentCulture) : string.Empty;
        }

        public static string LocalizedDateTime(this IHtmlHelper html, DateTime value)
        {
            return value.ToString("g", CultureInfo.CurrentCulture);
        }

        public static string IsoDateValue(this IHtmlHelper html, DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
        }

        public static string IsoDateValue(this IHtmlHelper html, DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static string JsDateFormat(this IHtmlHelper html)
        {
            return CultureInfo.CurrentCulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "mm/dd/yy" : "dd/mm/yy";
        }

        public static IHtmlContent LocalizedDateInput(this IHtmlHelper html, string name, DateTime? value, object htmlAttributes = null)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes ?? new { });
            attributes["type"] = "text";
            attributes["name"] = name;
            attributes["id"] = attributes.ContainsKey("id") ? attributes["id"] : name;
            attributes["value"] = html.LocalizedDate(value);
            attributes["data-date-format"] = html.JsDateFormat();
            attributes["autocomplete"] = "off";
            return html.TextBox(name, html.LocalizedDate(value), attributes);
        }
    }
}

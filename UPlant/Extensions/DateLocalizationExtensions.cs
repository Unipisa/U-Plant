using System;
using System.Globalization;
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

        public static string LocalizedDate(this IHtmlHelper html, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var invariantDate) ||
                DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out invariantDate))
            {
                return invariantDate.ToString("d", CultureInfo.CurrentCulture);
            }

            return value;
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

        public static string LocalizedDate<TModel>(this IHtmlHelper<TModel> html, DateTime? value)
        {
            return ((IHtmlHelper)html).LocalizedDate(value);
        }

        public static string LocalizedDate<TModel>(this IHtmlHelper<TModel> html, DateTime value)
        {
            return ((IHtmlHelper)html).LocalizedDate(value);
        }

        public static string LocalizedDate<TModel>(this IHtmlHelper<TModel> html, string value)
        {
            return ((IHtmlHelper)html).LocalizedDate(value);
        }

        public static string LocalizedDateTime<TModel>(this IHtmlHelper<TModel> html, DateTime? value)
        {
            return ((IHtmlHelper)html).LocalizedDateTime(value);
        }

        public static string LocalizedDateTime<TModel>(this IHtmlHelper<TModel> html, DateTime value)
        {
            return ((IHtmlHelper)html).LocalizedDateTime(value);
        }

        public static string IsoDateValue<TModel>(this IHtmlHelper<TModel> html, DateTime? value)
        {
            return ((IHtmlHelper)html).IsoDateValue(value);
        }

        public static string IsoDateValue<TModel>(this IHtmlHelper<TModel> html, DateTime value)
        {
            return ((IHtmlHelper)html).IsoDateValue(value);
        }

        public static string JsDateFormat<TModel>(this IHtmlHelper<TModel> html)
        {
            return ((IHtmlHelper)html).JsDateFormat();
        }

    }
}

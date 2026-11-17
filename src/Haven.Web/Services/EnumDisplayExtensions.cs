using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Haven.Web.Services;

public static class EnumDisplayExtensions
{
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public static string GetDisplayName(this Enum value) =>
        value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();

    public static DateTimeOffset ToVietnamTime(this DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, VietnamTimeZone);

    public static string ToVietnamDateTime(this DateTimeOffset value) => value.ToVietnamTime().ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
}

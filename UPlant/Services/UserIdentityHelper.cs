using System.Security.Claims;

namespace UPlant.Services;

public static class UserIdentityHelper
{
    public static string Normalize(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return string.Empty;
        }

        var value = username.Trim();
        var atIndex = value.IndexOf('@');
        return (atIndex >= 0 ? value.Substring(0, atIndex) : value).Trim();
    }

    public static string GetUsername(ClaimsPrincipal principal)
    {
        var username = principal?.FindFirst("UnipiUserID")?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            username = principal?.Identity?.Name;
        }

        return Normalize(username);
    }
}

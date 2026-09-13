using System.Text.RegularExpressions;

namespace OpenLedg.Domain;

internal static class Guard
{
    public static string Text(string value, int maxLength, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        value = value.Trim();
        if (value.Length > maxLength) throw new ArgumentException($"Maximum length: {maxLength}.", name);
        return value;
    }

    public static string Document(string value, string pattern, string name)
    {
        value = Text(value, 18, name).Replace(".", "").Replace("/", "").Replace("-", "").ToUpperInvariant();
        if (!Regex.IsMatch(value, pattern, RegexOptions.CultureInvariant))
            throw new ArgumentException("Invalid document format.", name);
        return value;
    }

    public static string Https(string value, string name)
    {
        value = Text(value, 2048, name);
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("An absolute HTTPS URL without user information is required.", name);
        return value;
    }
}

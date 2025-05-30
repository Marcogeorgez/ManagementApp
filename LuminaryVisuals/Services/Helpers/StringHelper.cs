using System.Text;
using System.Text.RegularExpressions;

namespace LuminaryVisuals.Services.Helpers;

public static class StringHelper
{

    /// <summary>
    /// Converts a string to title case by adding empty space between words.
    /// ex: "HelloWorld" -> "Hello World"
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Title Case String</returns>
    public static string AddSpacesBetweenCapitals(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        result.Append(input[0]); // Add the first character

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append(' '); // Add a space before the capital letter
            }
            result.Append(input[i]); // Add the current character
        }

        return result.ToString();
    }
    /// <summary>
    /// Checks if a given string is a link based on certain patterns.
    /// </summary>
    public static bool IsLink(string text)
    {
        return text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
               Regex.IsMatch(text, @"\.[a-z]{2,}$", RegexOptions.IgnoreCase);
    }
    private static readonly Regex UrlRegex = new Regex(
    @"https?:\/\/[^\s""']+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Splits input into text and links while retaining their order.
    /// </summary>
    public static IEnumerable<string> GetTextAndLinks(string input)
    {
        MatchCollection matches = UrlRegex.Matches(input);

        int currentIndex = 0;
        foreach (Match match in matches)
        {
            if (match.Index > currentIndex)
            {
                yield return input[currentIndex..match.Index];
            }

            yield return match.Value;

            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < input.Length)
        {
            yield return input[currentIndex..];
        }
    }
    /// <summary>
    /// Ensures the given URL starts with https://. Adds it if missing.
    /// </summary>
    public static string EnsureHttps(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // Add https:// if URL starts with "www."
            if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }
            // Otherwise, assume it's missing a scheme and add https://
            return "https://" + url;
        }

        // If the URL already has a scheme, return it unchanged
        return url;
    }
    public static string ProcessDeliverables(string deliverables)
    {
        if (string.IsNullOrEmpty(deliverables))
            return deliverables;
        if (deliverables.Contains("<img src"))
        {
            return deliverables;
        }
        // Pattern to match URLs, with negative lookbehind to avoid URLs in existing anchor tags
        string urlPattern = @"(?<!<a[^>]*>)(?!.*?</a>)(https?:\/\/[^<\s\n]+?)(?:[\s\n]|<br\s*\/?>|<\/p>)";

        // Replace URLs with anchor tags
        string processed = Regex.Replace(
            deliverables,
            urlPattern,
            match =>
            {
                string url = match.Groups[1].Value;  // Get the URL without the trailing space/newline/HTML break
                string endChar = match.Value.Substring(url.Length);  // Preserve the ending character(s)
                return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\">{url}</a>{endChar}";
            },
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );

        return processed;
    }
}
public static class ConvertLocal
{
    // Get the user local time to display the message in his own time.
    public static string ConvertToLocalTime(DateTime? utcTime, int timezoneOffsetMinutes)
    {
        if (utcTime == DateTime.MinValue || utcTime == null)
        {
            return string.Empty;
        }
        // Convert UTC time to user's local time based on the offset
        var localTime = utcTime.Value.AddMinutes(timezoneOffsetMinutes);
        return localTime.ToString("h:mm tt"); // Format for display
    }
    public static DateTime ConvertToLocalTimeDateTime(DateTime? utcTime, int timezoneOffsetMinutes)
    {
        if (utcTime == DateTime.MinValue || utcTime == null)
        {
            return DateTime.MinValue;
        }
        // Convert UTC time to user's local time based on the offset
        var localTime = utcTime.Value.AddMinutes(timezoneOffsetMinutes);
        return localTime; // Format for display
    }
}

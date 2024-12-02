using System;
using System.Text.RegularExpressions;

namespace LuminaryVisuals.Services.Helpers
{
    public static class StringHelper
    {
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
        /// <summary>
        /// Splits input into text and links while retaining their order.
        /// </summary>
        public static IEnumerable<string> GetTextAndLinks(string input)
        {
            string urlPattern = @"(https?://[^\s]+|www\.[^\s]+|[^\s]+\.com|[^\s]+\.org|[^\s]+\.net)";
            Regex regex = new Regex(urlPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(input);

            int currentIndex = 0;
            foreach (Match match in matches)
            {
                // Yield preceding text as plain text
                if (match.Index > currentIndex)
                {
                    // from current Index till match.Index
                    yield return input[currentIndex..match.Index];
                }

                // Yield the matched URL with scheme ensured
                if (IsLink(match.Value))
                {
                    yield return EnsureHttps(match.Value);
                }

                currentIndex = match.Index + match.Length;
            }

            // Yield any remaining text after the last match
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
    }
}

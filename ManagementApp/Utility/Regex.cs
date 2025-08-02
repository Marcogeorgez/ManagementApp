using System.Text.RegularExpressions;

namespace ManagementApp.Utility;

public static partial class MyRegexes
{
    [GeneratedRegex("<.*?>")]
    public static partial Regex HtmlCleanerRegex();
}
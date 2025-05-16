using System.Text.RegularExpressions;

namespace LuminaryVisuals.Utility;

public static partial class MyRegexes
{
    [GeneratedRegex("<.*?>")]
    public static partial Regex HtmlCleanerRegex();
}
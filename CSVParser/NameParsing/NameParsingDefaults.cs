using System.Text.RegularExpressions;

namespace CSVParser.NameParsing;

public enum NameParsingStandarts
{
    ExactMatch, 
    SnakeCase
}

/// <summary>
/// Standart functions to transform properties name into csv column names
/// It is suggested, that properties are written in PascalCase
/// In every function s stand for property name and the output value is the corresponding csv column name
/// </summary>
internal static class NameParsingDefaults
{
    public static string ExactMatch(string s) => s;

    public static string SnakeCase(string s)
    {
        return Regex.Replace(s, "[A-Z]", (Match match) =>
        {
            var strMatch = match.ToString().ToLower();
            if (match.Index > 0) strMatch = '_' + strMatch;
            return strMatch;
        });
    }
}

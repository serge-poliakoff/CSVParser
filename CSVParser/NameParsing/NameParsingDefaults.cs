using System.Text.RegularExpressions;

namespace CSVParser.NameParsing;

public enum NameParsingStandarts
{
    ExactMatch, 
    PascalToSnakeCase
}

/// <summary>
/// Standart functions to transform properties name into csv column names
/// It is suggested, that properties are written in PascalCase
/// In every function s stand for property name and the output value is the corresponding csv column name
/// </summary>
internal static class NameParsingDefaults
{
    public static string ExactMatch(string s) => s;

    public static string PascalToSnakeCase(string s)
    {
        return Regex.Replace(s, "[A-Z]", (Match match) =>
        {
            var strMatch = match.ToString().ToLower();
            if (match.Index > 0) strMatch = '_' + strMatch;
            return strMatch;
        });
    }
    public static string SnakeCaseToPascal(string s)
    {
        string res = s[0].ToString().ToUpper() + s.Substring(1);
        return Regex.Replace(res, "_[a-z]", (Match match) =>
        {
            return match.ToString().Substring(1).ToUpper();
        });
    }
    public static string SupressSpaces(string s)
    {
        return s.Replace(" ", "");
    }
}

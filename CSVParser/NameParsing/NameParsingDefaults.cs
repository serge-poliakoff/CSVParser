using System.Text.RegularExpressions;

namespace CSVParser.NameParsing;

public enum NameParsingStandarts
{
    ExactMatch, 
    PascalToSnakeCase,
    SnakeCaseToPascal,
    SupressSpaces,
    SupressCapitalise
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
        if (string.IsNullOrEmpty(s)) return s;
        return Regex.Replace(s, "[A-Z]", (Match match) =>
        {
            var strMatch = match.ToString().ToLower();
            if (match.Index > 0) strMatch = '_' + strMatch;
            return strMatch;
        });
    }
    public static string SnakeCaseToPascal(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        string res = s[0].ToString().ToUpper() + s.Substring(1);
        return Regex.Replace(res, "_[a-z]", (Match match) =>
        {
            return match.ToString().Substring(1).ToUpper();
        });
    }
    public static string SupressSpaces(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace(" ", "");
    }

    public static string SupressCapitalise(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s[0].ToString().ToUpper() + s.Substring(1).ToLower();
        return Regex.Replace(s, " [a-z]", (Match match) =>
        {
            return match.ToString().Substring(1).ToUpper();
        });
    }

    //todo: add supress spaces and capitalize policy for enums
}

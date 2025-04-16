
using System.Text.RegularExpressions;

namespace Saga.DomainShared.Helpers;

public static class StringHelper
{
    private static List<(string, string)> Rules
        =>
        [
            ("&", "__and"),
            ("'", "__ktp1"),
            ("\"", "__ktp2"),
            ("$", "__dlr"),
            ("@", "__at"),
            ("(", "__bk1"),
            (")", "__bk2"),
            ("[", "__kt1"),
            ("]", "__kt2"),
            ("{", "__kr1"),
            ("}", "__kr2"),
            ("|", "__lrs"),
            ("\\", "__gr1"),
            ("/", "__gr2"),
            ("?", "__tny"),
            ("!", "__sru"),
            ("#", "__pgr"),
            ("%", "__prc"),
            ("*", "__kl"),
            ("=", "__smd"),
            ("+", "__pls"),
            (":", "__ttk2"),
            (";", "__ttkk"),
            ("<", "__lbk"),
            (">", "__lbb"),
            (".", "__tik"),
            (",", "__km")
        ];
    public static string Limit(this string val, int limit = 100, string append = "...")
        => val.Length > limit ? string.Format("{0} {1}", val[..limit], append) : val;

    public static string Random(this string val, int length = 12)
        => new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890123456789", length)
            .Select(x => x[(new Random()).Next(x.Length)])
            .ToArray());

    public static string EnFilter(this string val)
    {
        foreach (var rule in Rules)
            val = val.Replace(rule.Item1, rule.Item2);
        return val;
    }

    public static string DeFilter(this string val)
    {
        foreach (var rule in Rules)
            val = val.Replace(rule.Item2, rule.Item1);
        return val;
    }

    public static string[] SplitCamelCase(this string val)
        => Regex.Split(val, @"(?<!^)(?=[A-Z])");
}

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 
/// </summary>
public class pathUtils
{
    private static Regex digitExtensionRegex = new(@"\.\d+$");

    public static bool isEquals(string path1, string path2)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            path1 = path1.Replace("\\", "/").ToLower().Trim();
            path2 = path2.Replace("\\", "/").ToLower().Trim();
            return path1 == path2;
        }
        else
        {
            path1 = path1.Replace("\\", "/").Trim();
            path2 = path2.Replace("\\", "/").Trim();
            return path1 == path2;
        }
    }

    public static bool isDigitExtension(string path)
    {
        return digitExtensionRegex.IsMatch(path);
    }

    public static string getDigitExtension(string path)
    {
        var match = digitExtensionRegex.Match(path);
        if (match.Success)
        {
            return match.Value;
        }
        return "";
    }

    public static string removeDigitExtension(string path)
    {
        return digitExtensionRegex.Replace(path, "");
    }

    public static string format(string path)
    {
        return path.Replace("\\", "/");
    }
}

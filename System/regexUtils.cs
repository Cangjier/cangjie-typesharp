using System.Text.RegularExpressions;

namespace Cangjie.TypeSharp.System;

public class regexUtils
{
    public static bool isValid(string pattern)
    {
        try {
            _ = new Regex(pattern);
            return true;
        }
        catch {
            return false;
        }
    }
}
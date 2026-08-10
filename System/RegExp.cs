using System.Text.RegularExpressions;
using TidyHPC.LiteJson;

namespace Cangjie.Typesharp.System;

/// <summary>
/// 正则表达式
/// </summary>
public class RegExp
{
    public Regex Regex { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="pattern">正则表达式</param>
    public RegExp(string pattern)
    {
        this.Regex = new Regex(pattern);
    }

    public RegExp(Json pattern, Json flagsOrOptions)
    {
        string patternString = pattern.AsString;
        RegexOptions options = RegexOptions.None;
        if (flagsOrOptions.IsString)
        {
            string flagsString = flagsOrOptions.AsString;
            if (flagsString.Contains("i"))
            {
                options |= RegexOptions.IgnoreCase;
            }

            if (flagsString.Contains("g"))
            {
                options |= RegexOptions.Multiline;
            }
        }
        else if (flagsOrOptions.IsObject)
        {
        }

        this.Regex = new Regex(patternString, options);
    }
}
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class JSON
{
    public static string stringify(object? value)
    {
        return new Json(value).ToString();
    }

    public static Json parse(string stringValue)
    {
        return Json.Parse(stringValue);
    }
}

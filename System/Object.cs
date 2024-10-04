using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class Object
{
    public static string[] keys(object value)
    {
        if (value == null)
        {
            return [];
        }
        if (value is Json jsonValue) return jsonValue.Keys;
        else return new Json(value).Keys;
    }
}

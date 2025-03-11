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

    public static Json values(Json value)
    {
        Json result = Json.NewArray();
        foreach (var item in value.Values)
        {
            result.Add(item);
        }
        return result;
    }
}

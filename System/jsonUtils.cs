using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class jsonUtils
{
    public static void replaceValue(Json value, Func<Json, Json> onValue)
    {
        value.EachAll(onValue);
    }

    public static void replaceKeyValue(Json value, Func<Json, Json, Json> onKeyValue)
    {
        value.EachAll((key, value) =>
        {
            return onKeyValue(key.ToJson(), value);
        });
    }
}

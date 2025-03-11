using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class jsonUtils
{
    public static void replaceValue(Json value,Func<Json,Json> onValue)
    {
        value.EachAll(onValue);
    }
}

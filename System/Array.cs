using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class Array : List<object?>
{
    public Array(Json length)
    {
        int int32Length = length.ToInt32;
        for (int i = 0; i < int32Length; i++)
        {
            Add(null);
        }
    }

    public static bool isArray(Json value)
    {
        return value.IsArray;
    }
}

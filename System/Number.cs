using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

public class Number
{
    public static bool isInteger(Json value)
    {
        if(value.IsInt32 && value.AsInt32!= int.MinValue)
        {
            return true;
        }
        return false;
    }

    public static bool isNaN(Json value)
    {
        if (value.IsInt32)
        {
            return value.AsInt32 == int.MinValue;
        }
        else if (value.IsFloat)
        {
            return float.IsNaN(value.AsFloat);
        }
        else if (value.IsDouble)
        {
            return double.IsNaN(value.AsDouble);
        }
        else if (value.IsString)
        {
            return double.TryParse(value.AsString, out double result) == false;
        }
        return false;
    }
}

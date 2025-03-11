using TidyHPC.LiteJson;
using SystemMath = System.Math;
namespace Cangjie.TypeSharp.System;

public class Math
{
    public static Json floor(Json x)
    {
        return (int)SystemMath.Floor(x.AsNumber);
    }

    public static Json ceil(Json x) 
    {
        return (int)SystemMath.Ceiling(x.AsNumber); 
    }

    private static Random Random = new Random();

    public static Json random()
    {
        return Random.NextDouble();
    }

    public static Json pow(Json x, Json y)
    {
        return SystemMath.Pow(x.AsNumber, y.AsNumber);
    }

    public static Json sqrt(Json x)
    {
        return SystemMath.Sqrt(x.AsNumber);
    }

    public static Json abs(Json x)
    {
        return SystemMath.Abs(x.AsNumber);
    }

    public static Json sin(Json x)
    {
        return SystemMath.Sin(x.AsNumber);
    }

    public static Json cos(Json x)
    {
        return SystemMath.Cos(x.AsNumber);
    }

    public static Json tan(Json x)
    {
        return SystemMath.Tan(x.AsNumber);
    }

    public static Json asin(Json x)
    {
        return SystemMath.Asin(x.AsNumber);
    }

    public static Json acos(Json x)
    {
        return SystemMath.Acos(x.AsNumber);
    }

    public static Json atan(Json x)
    {
        return SystemMath.Atan(x.AsNumber);
    }

    public static Json atan2(Json y, Json x)
    {
        return SystemMath.Atan2(y.AsNumber, x.AsNumber);
    }

    public static Json log(Json x)
    {
        return SystemMath.Log(x.AsNumber);
    }

    public static Json log10(Json x)
    {
        return SystemMath.Log10(x.AsNumber);
    }

    public static Json exp(Json x)
    {
        return SystemMath.Exp(x.AsNumber);
    }

    public static Json sign(Json x)
    {
        return SystemMath.Sign(x.AsNumber);
    }

    public static Json max(Json x, Json y)
    {
        return SystemMath.Max(x.AsNumber, y.AsNumber);
    }

    public static Json min(Json x, Json y)
    {
        return SystemMath.Min(x.AsNumber, y.AsNumber);
    }
}

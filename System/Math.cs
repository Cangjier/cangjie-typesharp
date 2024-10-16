using SystemMath = System.Math;
namespace Cangjie.TypeSharp.System;

public class Math
{
    public static int floor(double x)
    {
        return (int)SystemMath.Floor(x);
    }

    public static int ceil(double x) 
    {
        return (int)SystemMath.Ceiling(x); 
    }

    private static Random Random = new Random();

    public static double random()
    {
        return Random.NextDouble();
    }
}

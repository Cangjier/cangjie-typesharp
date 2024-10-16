namespace Cangjie.TypeSharp.System;

/// <summary>
/// File utilities
/// </summary>
public class fileUtils
{
    public static long getSize(string path)
    {
        return new FileInfo(path).Length;
    }
}

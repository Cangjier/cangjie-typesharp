namespace Cangjie.TypeSharp.System;
public class base64Utils
{
    public static string encodeString(string value)
    {
        return Convert.ToBase64String(Util.UTF8.GetBytes(value));
    }

    public static string encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static string decodeString(string value)
    {
        return Util.UTF8.GetString(Convert.FromBase64String(value));
    }

    public static byte[] decode(string value)
    {
        return Convert.FromBase64String(value);
    }
}

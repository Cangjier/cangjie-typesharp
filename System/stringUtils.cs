using Cangjie.TypeSharp.BasicType;
using System.Text;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public static class stringUtils
{
    public static string trimEnd(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trimEnd(value);
    }

    public static string trimStart(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trimStart(value);
    }

    public static string trim(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trim(value);
    }

    public static string trim(Json self)
    {
        return new JsonWrapper(self.Node).trim();
    }

    public static string changeEncoding(string value, int fromCodePage, int toCodePage)
    {
        // 获取源编码和目标编码
        Encoding fromEncoding = Encoding.GetEncoding(fromCodePage);
        Encoding toEncoding = Encoding.GetEncoding(toCodePage);
        
        // 将源字符串转换为字节数组
        byte[] fromBytes = fromEncoding.GetBytes(value);

        // 将字节数组转换为目标编码的字符串
        string result = toEncoding.GetString(fromBytes);

        return result;
    }

    public static string toString(Json value)
    {
        return value.Node?.ToString() ?? "";
    }

    public static string[] lines(Json value)
    {
        return value.AsString.Replace("\r", "").Split('\n');
    }
}

using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class datetimeUtils
{
    public static bool isSameWithSecond(DateTime left, DateTime right)
    {
        return left.Ticks / 10000000 == right.Ticks / 10000000;
    }

    public static bool isSameWithMillisecond(DateTime left, DateTime right)
    {
        return left.Ticks / 10000 == right.Ticks / 10000;
    }

    public static int getJSTimestamp(DateTime dateTime)
    {
        return (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static int getJSTimestamp()
    {
        return getJSTimestamp(DateTime.Now);
    }

    public static DateTime fromJSTimestamp(int timestamp)
    {
        return new DateTime(1970, 1, 1).AddSeconds(timestamp);
    }

    public static string toFormatString(Json value,string format)
    {
        return value.AsDateTime.ToString(format);
    }

    public static DateTime add(DateTime value,TimeSpan timeSpan)
    {
        return value.Add(timeSpan);
    }

    public static DateTime subtract(DateTime value, TimeSpan timeSpan)
    {
        return value.Subtract(timeSpan);
    }

    public static DateTime parse(Json value)
    {
        if (value.IsDateTime) return value.AsDateTime;
        else if (value.IsString) return DateTime.Parse(value.AsString);
        else if (value.IsNumber) return fromJSTimestamp(value.ToInt32);
        else throw new Exception($"无法解析为日期时间, {value}");
    }
}

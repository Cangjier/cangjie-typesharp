using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.System;
/// <summary>
/// 控制台
/// </summary>
#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
public class Consoles(Context context):IDisposable
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
{
    /// <summary>
    /// 输出日志
    /// </summary>
    /// <param name="value"></param>
    public void log(params object?[] values)
    {
        var result = values.Join(" ", TSRuntimeContext.AnyToString);
        Console.WriteLine(result);
        context.Logger.Info(result);
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="values"></param>
    public void error(params object[] values)
    {
        var result = values.Join(" ", TSRuntimeContext.AnyToString);
        Console.WriteLine(result);
        context.Logger.Error(result);
    }

    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="values"></param>
    public void debug(params object[] values)
    {
        var result = values.Join(" ", TSRuntimeContext.AnyToString);
        Console.WriteLine(result);
        context.Logger.Debug(result);
    }

    public void Dispose()
    {

    }
}

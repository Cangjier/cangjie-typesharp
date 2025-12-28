using System.Diagnostics;

namespace Cangjie.TypeSharp.System;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public class process
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    /// <summary>
    /// 列举所有进程
    /// </summary>
    /// <returns></returns>
    public static Process[] list()
    {
        return Process.GetProcesses();
    }

    /// <summary>
    /// 获取命令行
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    public static string cli(Process process)
    {
        return process.GetCommandLine();
    }
}

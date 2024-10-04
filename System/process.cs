using System.Diagnostics;

namespace TypeSharp.System;
public class process
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

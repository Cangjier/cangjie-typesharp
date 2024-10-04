using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 上下文
/// </summary>
#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
public static class context
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
{
    /// <summary>
    /// 参数
    /// </summary>
    public static string[] args = [];

    public static Json manifest = Json.NewObject();

    public static string script_path = "";

    public static object? @null = null;

    public static Json undefined = Json.Undefined;

    public static int exec(string path, params string[] args)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        args.Foreach(process.StartInfo.ArgumentList.Add);
        process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }

    public static async Task<int> execAsync(string path, params string[] args)
    {
        using var process = new Process();
        process.StartInfo.FileName = path;
        args.Foreach(process.StartInfo.ArgumentList.Add);
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    public static void start(string path, params string[] args)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        args.Foreach(process.StartInfo.ArgumentList.Add);
        process.Start();
    }

    public static int cmd(string workingDirectory, string commandLine)
    {
        // 局部函数：获取当前操作系统的shell
        string GetShell()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return "cmd.exe"; // Windows 下使用 cmd
            }
            else
            {
                return "/bin/bash"; // Linux/MacOS 下使用 bash
            }
        }

        // 局部函数：获取 shell 的参数
        string GetShellArguments(string command)
        {
            // 处理命令行中的双引号情况
            string escapedCommand = command.Replace("\"", "\\\"");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return $"/c {escapedCommand}"; // Windows 下 cmd 需要使用 /c 来执行命令
            }
            else
            {
                return $"-c \"{escapedCommand}\""; // Linux/MacOS 下 bash 使用 -c 参数
            }
        }

        try
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo startInfo = new()
            {
                FileName = GetShell(), // 根据系统获取合适的 shell
                Arguments = GetShellArguments(commandLine), // shell 的参数，包括命令行
                UseShellExecute = false,        // 启用 shell 执行，避免重定向
                CreateNoWindow = true,        // 允许创建窗口
                WorkingDirectory = workingDirectory // 设置工作目录
            };

            using Process process = new() { StartInfo = startInfo };
            // 启动进程
            process.Start();

            // 等待进程退出
            process.WaitForExit();

            // 返回进程的退出代码
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return -1; // 错误时返回 -1
        }

    }

    public static async Task<int> cmdAsync(string workingDirectory, string commandLine)
    {
        try
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo startInfo = new()
            {
                FileName = Util.GetShell(), // 根据系统获取合适的 shell
                Arguments = Util.GetShellArguments(commandLine), // shell 的参数，包括命令行
                UseShellExecute = false,        // 启用 shell 执行，避免重定向
                CreateNoWindow = true,        // 允许创建窗口
                WorkingDirectory = workingDirectory // 设置工作目录
            };

            using Process process = new() { StartInfo = startInfo };
            // 启动进程
            process.Start();

            // 等待进程退出
            await process.WaitForExitAsync();

            // 返回进程的退出代码
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return -1; // 错误时返回 -1
        }
    }

    public static async Task<int> cmdAsync(string workingDirectory, string commandLine,Json output)
    {
        try
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo startInfo = new()
            {
                FileName = Util.GetShell(), // 根据系统获取合适的 shell
                Arguments = Util.GetShellArguments(commandLine), // shell 的参数，包括命令行
                UseShellExecute = false,        // 启用 shell 执行，避免重定向
                CreateNoWindow = true,        // 允许创建窗口
                WorkingDirectory = workingDirectory, // 设置工作目录
                RedirectStandardOutput = true, // 重定向标准输出
            };

            using Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output.GetOrCreateArray("lines").Add(e.Data);
                }
            };
            // 启动进程
            process.Start();
            // 开始异步读取输出
            process.BeginOutputReadLine();
            // 等待进程退出
            await process.WaitForExitAsync();

            // 返回进程的退出代码
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return -1; // 错误时返回 -1
        }
    }

    public static void startCmd(string workingDirectory, string commandLine)
    {
        try
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo startInfo = new()
            {
                FileName = Util.GetShell(), // 根据系统获取合适的 shell
                Arguments = Util.GetShellArguments(commandLine), // shell 的参数，包括命令行
                UseShellExecute = false,        // 启用 shell 执行，避免重定向
                CreateNoWindow = true,        // 允许创建窗口
                WorkingDirectory = workingDirectory // 设置工作目录
            };

            Process process = new() { StartInfo = startInfo };
            // 启动进程
            process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static double parseFloat(string value)
    {
        return double.Parse(value);
    }

    public static int parseInt(string value)
    {
        return int.Parse(value);
    }

    public static string toString(object value)
    {
        return value.ToString() ?? string.Empty;
    }

    public static object Number(Json value)
    {
        if (value.IsString)
        {
            var valueString = value.AsString;
            // 尝试解析为整数
            if (int.TryParse(valueString, out int intValue))
            {
                return intValue;
            }
            else if (double.TryParse(valueString, out double doubleValue))
            {
                return doubleValue;
            }
            throw new Exception($"`{value}` 无法解析为数字");
        }
        else if(value.IsInt32)return value.AsInt32;
        else if (value.IsDouble) return value.AsDouble;
        throw new Exception($"`{value}` 无法解析为数字");
    }

    /// <summary>
    /// 拷贝文件夹
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="destinationDirectory"></param>
    /// <returns></returns>
    public static void copyDirectory(
        string sourceDirectory,
        string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine("Source directory does not exist.");
            return;
        }

        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        foreach (string item in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, item);
            var destination = Path.Combine(destinationDirectory, relativePath);
            if (Directory.Exists(destination) == false)
            {
                Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
            }
            else
            {
                // 如果目录是只读的，先取消只读
                var di = new DirectoryInfo(destination);
                if ((di.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        foreach (string item in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, item);
            var destination = Path.Combine(destinationDirectory, relativePath);
            // 如果文件是只读的，先取消只读
            if (File.Exists(destination))
            {
                var fi = new FileInfo(destination);
                if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fi.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
            File.Copy(item, Path.Combine(destinationDirectory, relativePath), true);
        }
    }

    public static void deleteFile(string sourcePath)
    {
        try
        {
            var fi = new FileInfo(sourcePath);
            if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                fi.Attributes &= ~FileAttributes.ReadOnly;
            }
            File.Delete(sourcePath);
        }
        catch
        {
            // 删除失败可能是什么原因？
        }
    }

    public static void deleteDirectory(string sourceDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine("Source directory does not exist.");
            return;
        }

        foreach (string item in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            deleteFile(item);
        }
        Directory.Delete(sourceDirectory, true);
    }

    public static void setLoggerPath(string path)
    {
        Logger.FilePath = path;
    }

    public static string getLoggerPath()
    {
        return Logger.FilePath;
    }

    private static ConcurrentDictionary<Guid, SemaphoreSlim> locks { get; } = [];

    public static void @lock(Guid id)
    {
        SemaphoreSlim? semaphore;
        lock (locks)
        {
            if (locks.TryGetValue(id, out semaphore) == false)
            {
                semaphore = new SemaphoreSlim(1, 1);
                locks[id] = semaphore;
            }
        }
        semaphore.Wait();
    }

    public static async Task lockAsync(Guid id)
    {
        SemaphoreSlim? semaphore;
        lock (locks)
        {
            if (locks.TryGetValue(id, out semaphore) == false)
            {
                semaphore = new SemaphoreSlim(1, 1);
                locks[id] = semaphore;
            }
        }
        await semaphore.WaitAsync();
    }

    public static void unlock(Guid id)
    {
        locks[id].Release();
    }
}

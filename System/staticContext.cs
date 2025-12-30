using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;
public static class staticContext
{
    public static object? @null = null;

    public static Json undefined = Json.Undefined;

    public static processResult exec(processConfig config)
    {
        var process = new Process();
        process.StartInfo.FileName = config.filePath;
        if (config.workingDirectory != string.Empty)
        {
            process.StartInfo.WorkingDirectory = config.workingDirectory;
        }
        process.StartInfo.UseShellExecute = config.useShellExecute;
        process.StartInfo.CreateNoWindow = config.createNoWindow;
        process.StartInfo.RedirectStandardOutput = config.redirect;
        process.StartInfo.RedirectStandardError = config.redirect;
        if (config.arguments.IsString)
        {
            process.StartInfo.Arguments = config.arguments.AsString;
        }
        else if (config.arguments.IsArray)
        {
            config.arguments.Foreach(item => process.StartInfo.ArgumentList.Add(item.AsString));
        }
        StringBuilder output = new();
        StringBuilder error = new();
        if (config.redirect)
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    error.AppendLine(e.Data);
                }
            };
        }
        process.Start();
        if (config.redirect)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        process.WaitForExit();
        var result = new processResult();
        if (config.redirect)
        {

            result.output = output.ToString();
            result.error = error.ToString();
        }
        else
        {

        }
        result.exitCode = process.ExitCode;
        return result;
    }

    public static async Task<processResult> execAsync(processConfig config)
    {
        var process = new Process();
        process.StartInfo.FileName = config.filePath;
        if (config.workingDirectory != string.Empty)
        {
            process.StartInfo.WorkingDirectory = config.workingDirectory;
        }
        process.StartInfo.UseShellExecute = config.useShellExecute;
        process.StartInfo.CreateNoWindow = config.createNoWindow;
        if (config.redirect)
        {
            process.StartInfo.RedirectStandardOutput = config.redirect;
            process.StartInfo.RedirectStandardError = config.redirect;
        }
        if (config.environment.IsObject)
        {
            foreach (var pair in config.environment.GetObjectEnumerable())
            {
                if (pair.Value.IsString)
                {
                    process.StartInfo.Environment[pair.Key] = pair.Value.AsString;
                }
                else if (pair.Value.IsObject)
                {
                    var environmentValue = pair.Value;
                    var action = environmentValue.Read("action", "");
                    if (action == "add")
                    {
                        process.StartInfo.Environment[pair.Key] = Environment.GetEnvironmentVariable(pair.Key) + ";" + environmentValue.Get("value", "").AsString;
                    }
                    else
                    {
                        process.StartInfo.Environment[pair.Key] = environmentValue.Get("value", "").AsString;
                    }
                }
                else if (pair.Value.IsArray)
                {
                    List<string> envItems = [];
                    foreach (var environmentValue in pair.Value.GetArrayEnumerable())
                    {
                        envItems.Add(environmentValue.AsString);
                    }
                    process.StartInfo.Environment[pair.Key] = envItems.Join(";");
                }
            }
        }
        if (config.arguments.IsString)
        {
            process.StartInfo.Arguments = config.arguments.AsString;
        }
        else if (config.arguments.IsArray)
        {
            config.arguments.Foreach(item => process.StartInfo.ArgumentList.Add(item.AsString));
        }
        List<string> output = [];
        List<string> error = [];
        if (config.redirect)
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output.Add(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    error.Add(e.Data);
                }
            };
        }
        process.Start();
        if (config.redirect)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        await process.WaitForExitAsync();
        var result = new processResult();
        if (config.redirect)
        {
            result.output = output.Join("\r\n");
            result.error = error.Join("\r\n");
        }
        else
        {

        }
        result.exitCode = process.ExitCode;
        return result;
    }

    public static int start(processConfig config)
    {
        var process = new Process();
        process.StartInfo.FileName = config.filePath;
        process.StartInfo.WorkingDirectory = config.workingDirectory;
        process.StartInfo.UseShellExecute = config.useShellExecute;
        config.arguments.Foreach(item => process.StartInfo.ArgumentList.Add(item.AsString));
        process.Start();
        return process.Id;
    }

    public static void kill(int pid)
    {
        var process = Process.GetProcessById(pid);
        process.Kill();
    }

    public static processResult cmd(string workingDirectory, string commandLine)
    {
        return exec(new processConfig
        {
            filePath = Util.GetShell(),
            workingDirectory = workingDirectory,
            arguments = new Json(Util.GetShellArguments(commandLine))
        });
    }

    public static processResult cmd(string workingDirectory, string commandLine, processConfig config)
    {
        return exec(new processConfig
        {
            filePath = Util.GetShell(),
            workingDirectory = workingDirectory,
            arguments = new Json(Util.GetShellArguments(commandLine)),
            useShellExecute = config.useShellExecute,
            createNoWindow = config.createNoWindow,
            redirect = config.redirect
        });
    }

    public static async Task<processResult> cmdAsync(string workingDirectory, string commandLine)
    {
        return await execAsync(new processConfig
        {
            filePath = Util.GetShell(),
            workingDirectory = workingDirectory,
            arguments = new Json(Util.GetShellArguments(commandLine))
        });
    }

    public static async Task<processResult> cmdAsync(string workingDirectory, string commandLine, processConfig config)
    {
        config.filePath = Util.GetShell();
        config.workingDirectory = workingDirectory;
        config.arguments = new Json(Util.GetShellArguments(commandLine));
        return await execAsync(config);
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
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        else
        {
            return int.MinValue;
        }
    }

    public static bool isNaN(Json value)
    {
        if (value.IsInt32)
        {
            return value.AsInt32 == int.MinValue;
        }
        else if (value.IsFloat)
        {
            return float.IsNaN(value.AsFloat);
        }
        else if (value.IsDouble)
        {
            return double.IsNaN(value.AsDouble);
        }
        else if (value.IsString)
        {
            return double.TryParse(value.AsString, out double result) == false;
        }
        return false;
    }

    public static string toString(object value)
    {
        return value.ToString() ?? string.Empty;
    }

    public static Json Number(Json value)
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
            return Int32.MinValue;
        }
        else if (value.IsInt32) return value.AsInt32;
        else if (value.IsDouble) return value.AsDouble;
        return Int32.MinValue;
    }

    public static string String(Json value)
    {
        if (value.IsString) return value.AsString;
        else if (value.IsInt32) return value.AsInt32.ToString();
        else if (value.IsDouble) return value.AsDouble.ToString();
        else if (value.IsBoolean) return value.AsBoolean ? "true" : "false";
        else if (value.IsNull) return "null";
        else if (value.IsUndefined) return "undefined";
        else return value.ToString();
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
            File.Delete(sourcePath);
            return;
        }
        catch
        {

        }
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
        try
        {
            Directory.Delete(sourceDirectory, true);
        }
        catch
        {

        }
    }

    public static void clearDirectory(string sourceDirectory)
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
        foreach (string item in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            if (Directory.Exists(item) == false)
            {
                continue;
            }
            deleteDirectory(item);
        }
    }

    public static string locate(string searchDirectory, string path)
    {

        var lastDirectory = searchDirectory;
        while (true)
        {
            if (lastDirectory == null)
            {
                return "";
            }
            var fullPath = Path.Combine(lastDirectory, path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            else if (Directory.Exists(fullPath))
            {
                return fullPath;
            }
            if (lastDirectory == Path.GetPathRoot(lastDirectory))
            {
                return "";
            }
            lastDirectory = Path.GetDirectoryName(lastDirectory);
        }
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

    public static bool lockFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool unlockFile(string filePath)
    {
        int retryCount = 3;
        bool success = false;
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                File.Delete(filePath);
                success = true;
                break;
            }
            catch
            {
                // 删除失败可能是什么原因？
            }
        }
        return success;
    }

    public static string env(string environmentVariable)
    {
        var lowerEnvironmentVariable = environmentVariable.ToLower();
        if (Environment.GetEnvironmentVariable(environmentVariable) is string result)
        {
            return result;
        }
        else if (lowerEnvironmentVariable == "desktop")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        else if (lowerEnvironmentVariable == "userprofile")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (lowerEnvironmentVariable == "appdata")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (lowerEnvironmentVariable == "mydocuments")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        return string.Empty;
    }

    public static string md5(Json value)
    {
        using var md5 = MD5.Create();
        if (value.IsString)
        {
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(value.AsString))).Replace("-", "").ToLower();
        }
        else
        {
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()))).Replace("-", "").ToLower();
        }
    }

    public static DateTime toDateTime(Json value)
    {
        if (value.Node is DateTime dateTime)
        {
            return dateTime;
        }
        else if (value.IsString)
        {
            return DateTime.Parse(value.AsString);
        }
        else if (value.IsInt32)
        {
            return new DateTime(value.AsInt32);
        }
        else if (value.IsDouble)
        {
            return new DateTime((long)value.AsDouble);
        }
        throw new Exception($"`{value}` 无法解析为日期");
    }

    public static Json programContext { get; } = new Json();
}

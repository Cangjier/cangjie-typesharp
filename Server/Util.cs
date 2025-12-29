using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Locks;

namespace Cangjie.TypeSharp.Server;

/// <summary>
/// 工具包
/// </summary>
public class Util
{
    /// <summary>
    /// 获取本地目录
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static string GetLocalDirectory(int parent = 0)
    {
        var result = Path.GetDirectoryName(Environment.ProcessPath) ?? Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? "";
        for (int i = 0; i < parent; i++)
        {
            result = Path.GetDirectoryName(result) ?? "";
        }
        return result;
    }

    /// <summary>
    /// Get the data directory
    /// </summary>
    /// <returns></returns>
    public static string GetDataDirectory()
        => GetSpecialDirectory("data");

    /// <summary>
    /// 获取特殊的目录
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public static string GetSpecialDirectory(string directoryName)
    {
        var localDirectory = Util.GetLocalDirectory();
        while (true)
        {
            var dataDirectory = Path.Combine(localDirectory, directoryName);
            if (Directory.Exists(dataDirectory))
            {
                return dataDirectory;
            }
            localDirectory = Path.GetDirectoryName(localDirectory);

            if (localDirectory == null || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(Util.GetLocalDirectory(), directoryName);
            }
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static string CreateDirectory(string? directory)
    {
        if (string.IsNullOrEmpty(directory)) return "";
        var parent = Path.GetDirectoryName(directory);
        if (!Directory.Exists(parent))
        {
            if (parent != null) CreateDirectory(parent);
        }
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return directory;
    }

    /// <summary>
    /// 转换为16进制字符串
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static string ConvertToHexString(byte[] hash)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取文件MD5
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static string GetFileMD5(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        using var md5Hash = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5Hash.ComputeHash(stream);
        return ConvertToHexString(hash);
    }

    /// <summary>
    /// 等待所有任务完成
    /// </summary>
    /// <param name="format"></param>
    /// <param name="funcs"></param>
    /// <returns></returns>
    public static async Task WhenAll(string format, params Func<Task>[] funcs)
    {
        Lock<SortedList<int, long>> times = new([]);
        Func<Task>[] wrappers = new Func<Task>[funcs.Length];
        var sw = Stopwatch.StartNew();
        // 对每一个func进行耗时统计
        for (int i = 0; i < funcs.Length; i++)
        {
            var index = i;
            var func = funcs[index];
            wrappers[index] = async () =>
            {
                var sw = Stopwatch.StartNew();
                await func();
                sw.Stop();
                times.Process(() => times.Value[index] = sw.ElapsedMilliseconds);
            };
        }
        await Task.WhenAll(wrappers.Select(wrapper => wrapper()));
        sw.Stop();
        var total = sw.ElapsedMilliseconds;
        _ = Task.Run(() =>
        {
            Console.WriteLine(format, total, times.Value.Values.Join(", ", item => $"{item}ms"));
        });
    }

    /// <summary>
    /// 等待所有任务完成
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task WhenAll(params Func<Task>[] func)
    {
        await Task.WhenAll(func.Select(f => f()));
    }

    /// <summary>
    /// 首字符大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        // 使用String.ToUpper方法将首字符转换为大写，然后使用String.Substring方法获取剩余部分  
        return char.ToUpper(str[0]) + str[1..];
    }

    private readonly static Regex ExceptionRegex = new(@"\s*--->\s*(?<exception>[^:]+):(?<message>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// 从日志中获取错误信息
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public static string? GetErrorMessageFromLogger(string[] lines)
    {
        bool isSystemException(string line)
        {
            return ExceptionRegex.IsMatch(line);
        }
        string getSystemExceptionMessage(string line)
        {
            return ExceptionRegex.Match(line).Groups["message"].Value;
        }
        const string loggerError = "[Error]";
        bool isLoggerError(string line)
        {
            return line.Contains(loggerError);
        }
        string getLoggerErrorMessage(string line)
        {
            return line.Substring(line.IndexOf(loggerError) + loggerError.Length).Trim();
        }
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (isSystemException(line))
            {
                return getSystemExceptionMessage(line);
            }
            if (isLoggerError(line))
            {
                return getLoggerErrorMessage(line);
            }
        }
        return null;
    }

    /// <summary>
    /// UTF8编码
    /// </summary>
    public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

    /// <summary>
    /// 获取所有网络IPv4地址
    /// </summary>
    /// <returns></returns>
    public static IPAddress[] GetNetworkIPAddressV4s()
    {
        List<IPAddress> result = [];
        // 获取本机所有网络接口的信息  
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface ni in networkInterfaces)
        {
            // 获取网络接口的所有IP地址信息  
            IPInterfaceProperties ipProperties = ni.GetIPProperties();

            // 遍历每个网络接口的所有Unicast地址（包括IPv4和IPv6）  
            foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
            {
                // IPv4地址  
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.Add(ip.Address);
                }
                // 如果需要IPv6地址，可以使用AddressFamily.InterNetworkV6  
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// 获取所有网络IPv4地址
    /// </summary>
    /// <returns></returns>
    public static IPAddress[] GetNetworkIPAddressV6s()
    {
        List<IPAddress> result = [];
        // 获取本机所有网络接口的信息  
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface ni in networkInterfaces)
        {
            // 获取网络接口的所有IP地址信息  
            IPInterfaceProperties ipProperties = ni.GetIPProperties();

            // 遍历每个网络接口的所有Unicast地址（包括IPv4和IPv6）  
            foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
            {
                // IPv4地址  
                if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    result.Add(ip.Address);
                }
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// 获取MD5
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetMD5(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (var t in hashBytes)
        {
            sb.Append(t.ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取子目录的根目录（及目标目录下存在文件或者多个目录，目标父目录下只存在一个目录）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetChildRootDirectory(string path)
    {
        var files = Directory.GetFiles(path);
        if (files.Length != 0) return path;
        var directories = Directory.GetDirectories(path);
        if (directories.Length != 1) return path;
        return GetChildRootDirectory(directories[0]);
    }

    /// <summary>
    /// 运行命令行
    /// </summary>
    /// <param name="workingDirectory"></param>
    /// <param name="commandLine"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<int> CmdAsync(string? workingDirectory, string commandLine, TimeSpan timeout)
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
            await process.WaitForExitAsync(new CancellationTokenSource(timeout).Token);

            // 返回进程的退出代码
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return -1; // 错误时返回 -1
        }
    }

    /// <summary>
    /// 获取 shell
    /// </summary>
    /// <returns></returns>
    public static string GetShell()
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

    /// <summary>
    /// 获取 shell 参数
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string GetShellArguments(string command)
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

    /// <summary>
    /// 检查文件是否被占用
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsFileInUse(string filePath)
    {
        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // 如果成功打开，说明文件未被占用
                return false;
            }
        }
        catch (IOException)
        {
            // 文件被占用
            return true;
        }
        catch (Exception)
        {
            // 其他异常也返回被占用（也可以根据需要再处理）
            return true;
        }
    }

    /// <summary>
    /// Axios
    /// </summary>
    public static class Axios
    {
        /// <summary>
        /// HTTP Client
        /// </summary>
        public static HttpClient HttpClient { get; } = new();

        private static async Task<string> Decode(HttpContent content)
        {
            if (content.Headers.ContentEncoding.Contains("gzip"))
            {
                using GZipStream stream = new(await content.ReadAsStreamAsync(), CompressionMode.Decompress);
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            else if (content.Headers.ContentEncoding.Contains("deflate"))
            {
                using DeflateStream stream = new(await content.ReadAsStreamAsync(), CompressionMode.Decompress);
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            else if (content.Headers.ContentEncoding.Contains("br"))
            {
                using BrotliStream stream = new(await content.ReadAsStreamAsync(), CompressionMode.Decompress);
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            return await content.ReadAsStringAsync();
        }

        /// <summary>
        /// 获取Message
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<NetMessageInterface> GetMessageAsync(string url)
        {
            HttpResponseMessage response = await HttpClient.GetAsync(url);
            return Json.Parse(await Decode(response.Content));
        }
    }

}

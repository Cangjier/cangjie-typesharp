using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// File utilities
/// </summary>
public class fileUtils
{
    public static long size(string path)
    {
        return new FileInfo(path).Length;
    }

    /// <summary>
    /// 带缓冲读取文件，获取文件字符串总长度（不一次性加载到内存）
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件字符总长度</returns>
    public static long stringLength(string path)
    {
        // 缓冲区大小：4KB（常用高效值，可调整）
        const int bufferSize = 4096;
        long totalLength = 0;

        // 流式读取文件，使用UTF-8编码（可根据需求修改编码）
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
        using (StreamReader reader = new StreamReader(fs, Util.UTF8))
        {
            // 字符缓冲区：只存放一次读取的字符
            char[] buffer = new char[bufferSize];
            int readCount;

            // 循环缓冲读取，直到文件末尾
            while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                // 累加本次读取到的字符数
                totalLength += readCount;
            }
        }

        return totalLength;
    }

    public static string md5(string path)
    {
        using var md5 = MD5.Create();
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
    }

    public static DateTime lastWriteTime(string path)
    {
        return File.GetLastWriteTime(path);
    }

    public static List<string> search(string directory, Regex pattern)
    {
        void _search(string directory, List<string> result)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (pattern.IsMatch(file))
                {
                    result.Add(file);
                }
            }
            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                _search(subDirectory, result);
            }
        }
        List<string> result = [];
        _search(directory, result);
        return result;
    }

    public static void writeLineWithShare(string path, string content)
    {
        using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        fileStream.Seek(0, SeekOrigin.End);
        fileStream.Write(Util.UTF8.GetBytes(content + "\r\n"));
    }

    public static bool isFileLocked(string path)
    {
        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch
        {
            return true;
        }
    }
}
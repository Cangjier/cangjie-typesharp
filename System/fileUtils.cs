using System.Security.Cryptography;
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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TidyHPC.LiteJson;

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

    private static ConcurrentDictionary<string, Regex> _regexCache = new();

    public static bool isMatch(string path, Json pattern)
    {
        Regex? regex = null;
        if (pattern.Is<Regex>())
        {
            regex = pattern.As<Regex>();
        }
        else if (pattern.IsString)
        {
            string patternString = pattern.AsString;
            if (_regexCache.TryGetValue(patternString, out regex) == false)
            {
                regex = new Regex(patternString);
                _regexCache.TryAdd(patternString, regex);
            }
        }
        else
        {
            throw new ArgumentException("Invalid pattern type");
        }

        return regex.IsMatch(File.ReadAllText(path, Util.UTF8));
    }

    private static readonly HashSet<string> _binaryExtensions =
    [
        // 可执行文件
        ".exe", ".dll", ".so", ".dylib", ".a", ".lib", ".o", ".obj",

        // 压缩/归档
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".zst",

        // 文档
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",

        // 图片
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg",

        // 音视频
        ".mp3", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".wav", ".flac", ".aac",

        // 其他二进制格式
        ".iso", ".img", ".bin", ".dat", ".db", ".mdb", ".pdb"
    ];

    private static bool tryMatchFileByLine(string file, Regex regex, Encoding encoding,
        [NotNullWhen(true)] out Match? match)
    {
        match = null;
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var streamReader = new StreamReader(fileStream, encoding);
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();
            if (line == null)
            {
                break;
            }

            match = regex.Match(line);
            if (match.Success)
            {
                return true;
            }
        }

        return false;
    }

    public static Json search(string directory, Json pattern, Json options = default)
    {
        Encoding encoding = Util.UTF8;
        bool searchByPath = true;
        bool searchByContent = false;
        int pageIndex = 0;
        int pageSize = 20;
        if (options.IsObject)
        {
            if (options.TryGet("encoding", out var encodingValue))
            {
                if (encodingValue.IsString)
                {
                    encoding = encodingValue.AsString switch
                    {
                        "utf8" => Util.UTF8,
                        "gbk" => Encoding.GetEncoding("gbk"),
                        _ => throw new ArgumentException($"Invalid encoding: {encodingValue.AsString}")
                    };
                }
                else if (encodingValue.Is<Encoding>())
                {
                    encoding = encodingValue.As<Encoding>();
                }
                else
                {
                    throw new ArgumentException($"Invalid encoding: {encodingValue}");
                }
            }

            if (options.TryGet("searchByPath", out var searchByPathValue))
            {
                searchByPath = searchByPathValue.AsBoolean;
            }

            if (options.TryGet("searchByContent", out var searchByContentValue))
            {
                searchByContent = searchByContentValue.AsBoolean;
            }

            if (options.TryGet("pageIndex", out var pageIndexValue))
            {
                pageIndex = pageIndexValue.ToInt32;
            }

            if (options.TryGet("pageSize", out var pageSizeValue))
            {
                pageSize = pageSizeValue.ToInt32;
            }
        }

        var startIndex = pageIndex * pageSize;
        var endIndex = startIndex + pageSize;

        Regex? regex;
        if (pattern.Is<Regex>())
        {
            regex = pattern.As<Regex>();
        }
        else if (pattern.IsString)
        {
            string patternString = pattern.AsString;
            if (_regexCache.TryGetValue(patternString, out regex) == false)
            {
                regex = new Regex(patternString);
                _regexCache.TryAdd(patternString, regex);
            }
        }
        else
        {
            throw new ArgumentException("Invalid pattern type");
        }

        Json result = Json.NewArray();
        int currentIndex = 0;
        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
        foreach (var fileChunk in files.Chunk(100))
        {
            foreach (var file in fileChunk)
            {
                var formatPath = file.Replace(directory, "").Replace("\\", "/");
                if (formatPath.Contains("/.git/") ||
                    formatPath.Contains("/.svn/") ||
                    formatPath.Contains("/.hg/")  ||
                    formatPath.Contains("/.bzr/") ||
                    formatPath.Contains("/.cvs/"))
                {
                    continue;
                }

                if (currentIndex >= endIndex)
                {
                    break;
                }

                if (searchByContent)
                {
                    var fileExtension = Path.GetExtension(file);
                    if (_binaryExtensions.Contains(fileExtension) == false)
                    {
                        if (tryMatchFileByLine(file, regex, encoding, out var matchResult))
                        {
                            currentIndex++;
                            if (currentIndex >= startIndex && currentIndex < endIndex)
                            {
                                result.Add(Json.NewObject()
                                    .Set("path", file)
                                    .Set("match", matchResult.Value)
                                    .Set("from", "content")
                                );
                            }

                            continue;
                        }
                    }
                }

                if (searchByPath)
                {
                    var matchResult = regex.Match(file);
                    if (matchResult.Success)
                    {
                        currentIndex++;
                        if (currentIndex >= startIndex && currentIndex < endIndex)
                        {
                            result.Add(Json.NewObject()
                                .Set("path", file)
                                .Set("match", matchResult.Value)
                                .Set("from", "path")
                            );
                        }

                        continue;
                    }
                }
            }
        }

        return result;
    }
}
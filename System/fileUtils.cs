using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cangjie.Typesharp.System;
using TidyHPC.Extensions;
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
        if (File.Exists(path))
        {
            return File.GetLastWriteTime(path);
        }
        else if (Directory.Exists(path))
        {
            return Directory.GetLastWriteTime(path);
        }
        else
        {
            throw new FileNotFoundException($"File or directory not found: {path}");
        }
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

    public static int getFilesCount(string path, SearchOption searchOption = SearchOption.AllDirectories)
    {
        try
        {
            return Directory.EnumerateFiles(path, "*", searchOption).Count();
        }
        catch
        {
            return -1;
        }
    }

    public static string[] getFiles(string path, int pageNumber, int pageSize,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        var files = Directory.EnumerateFiles(path, "*", searchOption);
        return files.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
    }

    public static string[] getDirectories(string path, int pageNumber, int pageSize,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        var directories = Directory.EnumerateDirectories(path, "*", searchOption);
        return directories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
    }

    public static string[] getFilesAndDirectories(string path, int pageNumber, int pageSize,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        var filesAndDirectories = Directory.EnumerateFileSystemEntries(path, "*", searchOption);
        return filesAndDirectories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
    }

    public static string[] getFilesByExtensions(string path, string[] extensions, int pageNumber, int pageSize,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        var files = Directory.EnumerateFiles(path, $"*{extensions.Join(",")}", searchOption);
        return files.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
    }

    public static int getDirectoriesCount(string path, SearchOption searchOption = SearchOption.AllDirectories)
    {
        try
        {
            return Directory.EnumerateDirectories(path, "*", searchOption).Count();
        }
        catch
        {
            return -1;
        }
    }

    public static string? getSingleFile(string path)
    {
        if (Directory.Exists(path) == false)
        {
            return null;
        }

        var directories = Directory.GetDirectories(path);
        if (directories.Length > 1)
        {
            return null;
        }

        var files = Directory.GetFiles(path);
        if (directories.Length == 0 && files.Length == 1)
        {
            return files[0];
        }
        else if (directories.Length == 1 && files.Length == 0)
        {
            return getSingleFile(directories[0]);
        }

        return null;
    }

    public static int getChildrenCount(string path, SearchOption searchOption = SearchOption.AllDirectories)
    {
        try
        {
            return Directory.EnumerateFileSystemEntries(path, "*", searchOption).Count();
        }
        catch
        {
            return -1;
        }
    }

    public static Dictionary<string, int> getFileCountByExtension(string path, string[] extensions,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        var allFiles = Directory.EnumerateFiles(path, "*", searchOption);
        var result = new Dictionary<string, int>();
        var extensionSet = new HashSet<string>(extensions);
        foreach (var files in allFiles.Chunk(100))
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (extensionSet.Contains(extension))
                {
                    if (result.TryGetValue(extension, out var count))
                    {
                        result[extension] = count + 1;
                    }
                    else
                    {
                        result[extension] = 1;
                    }
                }
            }
        }

        return result;
    }

    public static string? getTopContent(string path, int maxLength)
    {
        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(fileStream, Util.UTF8);
            StringBuilder content = new();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line == null)
                {
                    break;
                }

                content.AppendLine(line);
                if (content.Length > maxLength)
                {
                    break;
                }
            }

            return content.ToString().Substring(0, global::System.Math.Min(content.Length, maxLength));
        }
        catch
        {
            return null;
        }
    }

    public static string? getRangeContent(string path, int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < startIndex)
            return string.Empty;

        try
        {
            using var reader = new StreamReader(path, Util.UTF8);

            var sb = new StringBuilder();
            var buffer = new char[4096];
            int index = 0;
            int read;

            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    if (index >= startIndex && index <= endIndex)
                    {
                        sb.Append(buffer[i]);
                    }

                    index++;

                    if (index > endIndex)
                    {
                        return sb.ToString();
                    }
                }
            }

            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    public static string matchLines(string path, RegExp regex)
    {
        try
        {
            List<string> lines = new();
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(fileStream, Util.UTF8);
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line == null)
                {
                    break;
                }

                if (regex.Regex.IsMatch(line))
                {
                    lines.Add(line);
                }
            }

            return lines.Join("\n");
        }
        catch
        {
            return "";
        }
    }

    public static bool containsFileName(string path, string fileName,
        StringComparison stringComparison = StringComparison.Ordinal)
    {
        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
        try
        {
            foreach (var chunk in files.Chunk(100))
            {
                foreach (var file in chunk)
                {
                    if (string.Compare(Path.GetFileName(file), fileName, stringComparison) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool containsNumberOfFiles(string path, int numberOfFiles)
    {
        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
        try
        {
            int count = 0;
            foreach (var chunk in files.Chunk(100))
            {
                count += chunk.Length;
                if (count >= numberOfFiles)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static string getFileSizeAlias(Json sizeOrPath)
    {
        try
        {
            long size;
            if (sizeOrPath.IsString)
            {
                size = new FileInfo(sizeOrPath.AsString).Length;
            }
            else if (sizeOrPath.IsNumber)
            {
                size = sizeOrPath.ToInt64;
            }
            else
            {
                throw new ArgumentException($"Invalid size or path: {sizeOrPath}");
            }

            if (size < 1024)
            {
                return $"{size}B";
            }
            else if (size < 1024 * 1024)
            {
                return $"{size / 1024}KB{getFileSizeAlias(size % 1024)}";
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return $"{size / 1024 / 1024}MB{getFileSizeAlias(size % (1024 * 1024))}";
            }
            else
            {
                return $"{size / 1024 / 1024 / 1024}GB{getFileSizeAlias(size % (1024 * 1024 * 1024))}";
            }
        }
        catch
        {
            return "???B";
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
        try
        {
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
        catch
        {
            return false;
        }
    }

    public static Json search(string directory, Json pattern, Json options = default)
    {
        Encoding encoding = Util.UTF8;
        bool searchByPath = true;
        bool searchByContent = false;
        int pageIndex = 0;
        int pageSize = 20;
        Regex? includeFileNameRegex = null;
        Regex? excludeFileNameRegex = null;
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

            if (options.TryGet("includeFileNameRegex", out var includeFileNameRegexValue))
            {
                if (includeFileNameRegexValue.IsString)
                {
                    var includeFileNameRegexString = includeFileNameRegexValue.AsString;
                    if (includeFileNameRegexString != "null" && includeFileNameRegexString != "")
                    {
                        includeFileNameRegex = new Regex(includeFileNameRegexString);
                    }
                }
            }

            if (options.TryGet("excludeFileNameRegex", out var excludeFileNameRegexValue))
            {
                if (excludeFileNameRegexValue.IsString)
                {
                    var excludeFileNameRegexString = excludeFileNameRegexValue.AsString;
                    if (excludeFileNameRegexString != "null" && excludeFileNameRegexString != "")
                    {
                        excludeFileNameRegex = new Regex(excludeFileNameRegexString);
                    }
                }
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
                    formatPath.Contains("/.hg/") ||
                    formatPath.Contains("/.bzr/") ||
                    formatPath.Contains("/.cvs/"))
                {
                    continue;
                }

                if (includeFileNameRegex != null && includeFileNameRegex.IsMatch(formatPath) == false)
                {
                    continue;
                }

                if (excludeFileNameRegex != null && excludeFileNameRegex.IsMatch(formatPath))
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
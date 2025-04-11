using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// ArchiveDirectory
/// </summary>
public class ArchiveDirectory(Archive archive, ZipArchiveEntry? zipArchiveEntry, string filePath)
{
    /// <summary>
    /// 获取归档对象
    /// </summary>
    public Archive Archive { get; } = archive;

    /// <summary>
    /// 获取ZIP归档条目
    /// </summary>
    public ZipArchiveEntry? ZipArchiveEntry { get; } = zipArchiveEntry;

    /// <summary>
    /// 获取文件路径
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// 判断目标文件/目录是否存在
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否存在</returns>
    public bool Contains(string fileName)
    {
        if (FilePath == "/" || FilePath == "")
        {
            return Archive.ZipArchive.Entries.Any(entry => entry.FullName == fileName);
        }
        else
        {
            return Archive.ZipArchive.Entries.Any(entry => entry.FullName == FilePath + "/" + fileName);
        }
    }

    /// <summary>
    /// 获取当前目录下的所有文件
    /// </summary>
    /// <returns>文件数组</returns>
    public ArchiveFile[] GetFiles()
    {
        if (FilePath == "/" || FilePath == "")
        {
            return Archive.GetFiles();
        }
        else
        {
            return Archive.GetFilesByPath(FilePath);
        }
    }

    /// <summary>
    /// 获取当前目录下的所有子目录
    /// </summary>
    /// <returns>目录数组</returns>
    public ArchiveDirectory[] GetDirectories()
    {
        if (FilePath == "/" || FilePath == "")
        {
            return Archive.GetDirectories();
        }
        else
        {
            return Archive.GetDirectoriesByPath(FilePath);
        }
    }

    /// <summary>
    /// 在当前目录下获取或创建文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>文件对象</returns>
    public ArchiveFile GetOrCreateFile(string fileName)
    {
        if (fileName.EndsWith('/'))
        {
            throw new ArgumentException("File name should not end with /");
        }
        if (FilePath == "/" || FilePath == "")
        {
            return Archive.GetOrCreateFileByPath(fileName);
        }
        else
        {
            if (FilePath.EndsWith('/') == false)
            {
                return Archive.GetOrCreateFileByPath(FilePath + "/" + fileName);
            }
            else
            {
                return Archive.GetOrCreateFileByPath(FilePath + fileName);
            }
        }
    }

    /// <summary>
    /// 在当前目录下获取或创建子目录
    /// </summary>
    /// <param name="directoryName">目录名</param>
    /// <returns>目录对象</returns>
    public ArchiveDirectory GetOrCreateDirectory(string directoryName)
    {
        if (directoryName.EndsWith('/') == false)
        {
            directoryName += "/";
        }
        if (FilePath == "/" || FilePath == "")
        {
            return Archive.GetOrCreateDirectoryByPath(directoryName);
        }
        else
        {
            if (FilePath.EndsWith('/') == false)
            {
                return Archive.GetOrCreateDirectoryByPath(FilePath + "/" + directoryName);
            }
            else
            {
                return Archive.GetOrCreateDirectoryByPath(FilePath + directoryName);
            }
        }
    }

    /// <summary>
    /// 删除当前目录及其下所有文件和子目录
    /// </summary>
    public void Delete()
    {
        if (FilePath == "/" || FilePath == "")
        {
            Archive.ZipArchive.Entries.Foreach(entry => entry.Delete());
        }
        else
        {
            ZipArchiveEntry[] allEntries = [.. Archive.ZipArchive.Entries.Where(entry =>
                entry.FullName.StartsWith(FilePath)
            )];
            // 将目录下所有文件/目录删除
            allEntries.Foreach(file => file.Delete());
        }
    }
}

/// <summary>
/// ArchiveFile
/// </summary>
public class ArchiveFile(Archive archive, ZipArchiveEntry zipArchiveEntry, string filePath)
{
    /// <summary>
    /// 获取或设置归档对象
    /// </summary>
    public Archive Archive { get; } = archive;

    /// <summary>
    /// 获取或设置ZIP归档条目
    /// </summary>
    public ZipArchiveEntry ZipArchiveEntry { get; } = zipArchiveEntry;

    /// <summary>
    /// 获取或设置文件路径
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// 打开文件流
    /// </summary>
    /// <returns>文件流</returns>
    public Stream Open()
    {
        return ZipArchiveEntry.Open();
    }

    /// <summary>
    /// 写入流数据
    /// </summary>
    /// <param name="stream">要写入的流</param>
    public void WriteStream(Stream stream)
    {
        using var entryStream = ZipArchiveEntry.Open();
        entryStream.SetLength(0);
        entryStream.Position = 0;
        stream.CopyTo(entryStream);
        entryStream.Flush();
    }

    /// <summary>
    /// 写入字符串内容
    /// </summary>
    /// <param name="content">要写入的字符串</param>
    public void WriteString(string content)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        WriteStream(stream);
    }

    /// <summary>
    /// 写入字节数组
    /// </summary>
    /// <param name="buffer">要写入的字节数组</param>
    public void WriteBytes(byte[] buffer)
    {
        using var stream = new MemoryStream(buffer);
        WriteStream(stream);
    }

    /// <summary>
    /// 以字符串形式读取文件内容
    /// </summary>
    /// <returns>文件内容字符串</returns>
    public string ReadAsString()
    {
        using var stream = Open();
        using var reader = new StreamReader(stream, Archive.DefaultEncoding);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 以字节数组形式读取文件内容
    /// </summary>
    /// <returns>文件内容字节数组</returns>
    public byte[] ReadAsBytes()
    {
        using var stream = Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public void Delete()
    {
        ZipArchiveEntry.Delete();
    }
}

/// <summary>
/// <para>ZipArchive是面向entry list的，Archive是面向file/directory的</para>
/// </summary>  
/// <param name="stream">流</param>
/// <param name="zipArchive">ZipArchive</param>
/// <param name="filePath">文件路径</param>
/// <param name="canBeDisposed">是否可以释放</param>
public class Archive : IDisposable
{
    public static Encoding DefaultEncoding { get; } = new UTF8Encoding(false);

    public Archive(FileStream stream, string filePath, bool canBeDisposed = false)
    {
        Stream = stream;
        FilePath = filePath;
        CanBeDisposed = canBeDisposed;
        ZipArchive = new ZipArchive(stream, ZipArchiveMode.Update, true);
    }
    public FileStream Stream { get; }

    /// <summary>
    /// ZipArchive
    /// </summary>
    public ZipArchive ZipArchive { get; private set; }
    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 是否可以释放
    /// </summary>
    private bool CanBeDisposed { get; }

    /// <summary>
    /// 打开一个zip文件
    /// </summary>
    public static Archive Open(string filePath)
    {
        // 如果filePath不存在，则创建ziparchive
        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        return new Archive(fileStream, filePath, true);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (CanBeDisposed)
        {
            ZipArchive.Dispose();
            Stream.Dispose();
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        ZipArchive.Dispose();
        Stream.Flush(true);
        Stream.Position = 0;
        ZipArchive = new ZipArchive(Stream, ZipArchiveMode.Update, true);
    }

    /// <summary>
    /// 获取根目录
    /// </summary>
    /// <returns></returns>
    public ArchiveDirectory GetRootDirectory()
    {
        return new ArchiveDirectory(this, null, "/");
    }

    /// <summary>
    /// 获取跟目录下的文件
    /// </summary>
    /// <returns></returns>
    public ArchiveFile[] GetFiles()
    {
        return [.. ZipArchive.Entries.Where(entry => entry.FullName.Contains('/') == false).Select(entry => new ArchiveFile(this, entry, entry.FullName))];
    }

    /// <summary>
    /// 获取跟目录下的目录
    /// </summary>
    /// <returns></returns>
    public ArchiveDirectory[] GetDirectories()
    {
        return [.. ZipArchive.Entries.Where(entry =>
            entry.FullName.EndsWith('/') &&
            entry.FullName[..^1].Contains('/') == false)
            .Select(entry => new ArchiveDirectory(this, entry, entry.FullName))];
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    public ArchiveFile GetFileByPath(string path)
    {
        if (path.EndsWith('/'))
        {
            throw new ArgumentException("Path should not end with /");
        }
        // 判断是否存在，不存在直接抛异常
        if (!ZipArchive.Entries.Any(entry => entry.FullName == path))
        {
            throw new FileNotFoundException($"File {path} not found in {FilePath}");
        }
        return new ArchiveFile(this, ZipArchive.Entries.First(entry => entry.FullName == path), path);
    }

    /// <summary>
    /// 获取或创建文件
    /// </summary>
    public ArchiveFile GetOrCreateFileByPath(string path)
    {
        if (path.EndsWith('/'))
        {
            throw new ArgumentException("Path should not end with /");
        }
        // 判断是否存在，不存在则创建
        if (!ZipArchive.Entries.Any(entry => entry.FullName == path))
        {
            ZipArchive.CreateEntry(path);
        }
        return new ArchiveFile(this, ZipArchive.Entries.First(entry => entry.FullName == path), path);
    }

    /// <summary>
    /// 获取目录
    /// </summary>
    public ArchiveDirectory GetDirectoryByPath(string path)
    {
        if (path.EndsWith('/') == false)
        {
            path += "/";
        }
        if (path == "/")
        {
            return new ArchiveDirectory(this, null, path);
        }
        // 判断是否存在，不存在直接抛异常
        if (!ZipArchive.Entries.Any(entry => entry.FullName == path))
        {
            throw new DirectoryNotFoundException($"Directory {path} not found in {FilePath}");
        }
        return new ArchiveDirectory(this, ZipArchive.Entries.First(entry => entry.FullName == path), path);
    }

    /// <summary>
    /// 获取或创建目录
    /// </summary>
    public ArchiveDirectory GetOrCreateDirectoryByPath(string path)
    {
        if (path.EndsWith('/') == false)
        {
            path += "/";
        }
        if (path == "/")
        {
            return new ArchiveDirectory(this, null, path);
        }
        // 判断是否存在，不存在则创建
        if (!ZipArchive.Entries.Any(entry => entry.FullName == path))
        {
            ZipArchive.CreateEntry(path);
        }
        return new ArchiveDirectory(this, ZipArchive.Entries.First(entry => entry.FullName == path), path);
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ArchiveFile[] GetFilesByPath(string path)
    {
        if (path.EndsWith('/') == false)
        {
            path += "/";
        }
        return [.. ZipArchive.Entries.Where(entry =>
        entry.FullName.StartsWith(path) &&
        entry.FullName.EndsWith('/') == false &&
        entry.FullName[path.Length..].Contains('/') == false)
        .Select(entry => new ArchiveFile(this, entry, entry.FullName))];
    }

    /// <summary>
    /// 获取目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ArchiveDirectory[] GetDirectoriesByPath(string path)
    {
        if (path.EndsWith('/') == false)
        {
            path += "/";
        }
        return [.. ZipArchive.Entries.Where(entry =>
        entry.FullName.StartsWith(path) &&
        entry.FullName.EndsWith('/') &&
        entry.FullName[path.Length..^1].Contains('/') == false)
        .Select(entry => new ArchiveDirectory(this, entry, entry.FullName))];
    }

}

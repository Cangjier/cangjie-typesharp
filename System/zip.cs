using System.IO.Compression;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// zip压缩
/// </summary>
public class zip
{
    public static async Task extract(string zipPath,string extractDirectory)
    {
        using ZipArchive archive = ZipFile.OpenRead(zipPath);
        // 先判断每一个entry的根目录是否同一个，如果是同一个，则直接解压到同一个目录下
        string? rootDirectory = null;
        bool isSameRootDirectory = true;
        string removeRootDirectory(string fullName)
        {
            if (isSameRootDirectory&& rootDirectory!=null)
            {
                return fullName[(rootDirectory!.Length + 1)..];
            }
            return fullName;
        }
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string directory = entry.FullName.Split('/')[0];
            if (rootDirectory == null)
            {
                rootDirectory = directory;
            }
            else if (rootDirectory != directory)
            {
                isSameRootDirectory = false;
                break;
            }
        }
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string fullPath = Path.Combine(extractDirectory, removeRootDirectory(entry.FullName));
            if (entry.FullName.EndsWith("/"))
            {
                Directory.CreateDirectory(fullPath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                using Stream stream = entry.Open();
                using FileStream fileStream = new(fullPath, FileMode.Create);
                await stream.CopyToAsync(fileStream);
            }
        }
    }

    public static async Task compress(string directoryPath, string zipPath)
    {
        using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            var entry = archive.CreateEntry(file.Replace(directoryPath, "").TrimStart('\\', '/'));
            using Stream stream = File.OpenRead(file);
            using Stream entryStream = entry.Open();
            await stream.CopyToAsync(entryStream);
        }
    }
}

using System.IO.Compression;

namespace TypeSharp.System;

/// <summary>
/// zip压缩
/// </summary>
public class zip
{
    public static async Task extract(string zipPath,string extractDirectory)
    {
        using ZipArchive archive = ZipFile.OpenRead(zipPath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string fullPath = Path.Combine(extractDirectory, entry.FullName);
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

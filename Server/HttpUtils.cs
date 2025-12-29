using System.IO.Compression;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.Server;

/// <summary>
/// Http工具
/// </summary>
public class HttpUtils
{
    private static HttpClient HttpClient { get; } = new();

    /// <summary>
    /// Get请求，返回附件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="downloadDirectory"></param>
    /// <returns></returns>
    public static async Task<string> GetAttachmentAsync(string url,string downloadDirectory)
    {
        // FileName 从响应Headers的Content-Disposition中获取
        using HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        using HttpResponseMessage responseMessage = await HttpClient.SendAsync(requestMessage);
        using Stream stream = await responseMessage.Content.ReadAsStreamAsync();
        string? fileName = responseMessage.Content.Headers.ContentDisposition?.FileName;
        if (fileName == null)
        {
            Logger.Error($"url={url}");
            Logger.Error($"downloadDirectory={downloadDirectory}");
            throw new Exception($"Content-Disposition中未找到文件名");
        }
        // 如果fileName 包含双引号，去掉双引号
        fileName = fileName.Trim('"');
        string filePath = Path.Combine(downloadDirectory, fileName);
        using FileStream fileStream = new(filePath, FileMode.Create);
        var contentEncoding = responseMessage.Content.Headers.ContentEncoding;
        if (contentEncoding.Contains("gzip"))
        {
            using GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(fileStream);
        }
        else if (contentEncoding.Contains("deflate"))
        {
            using DeflateStream deflateStream = new(stream, CompressionMode.Decompress);
            await deflateStream.CopyToAsync(fileStream);
        }
        else if (contentEncoding.Contains("br"))
        {
            using BrotliStream brotliStream = new(stream, CompressionMode.Decompress);
            await brotliStream.CopyToAsync(fileStream);
        }
        else
        {
            await stream.CopyToAsync(fileStream);
        }
        return filePath;
    }

    /// <summary>
    /// Get请求，返回Json
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<Json> GetJsonAsync(string url)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        using HttpResponseMessage response = await HttpClient.SendAsync(request);
        // 根据Content-Encoding解压缩
        using Stream stream = await response.Content.ReadAsStreamAsync();
        if(response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            using GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            return await Json.ParseAsync(gzipStream);
        }
        else if(response.Content.Headers.ContentEncoding.Contains("deflate"))
        {
            using DeflateStream deflateStream = new(stream, CompressionMode.Decompress);
            return await Json.ParseAsync(deflateStream);
        }
        else if(response.Content.Headers.ContentEncoding.Contains("br"))
        {
            using BrotliStream brotliStream = new(stream, CompressionMode.Decompress);
            return await Json.ParseAsync(brotliStream);
        }
        else
        {
            return await Json.ParseAsync(stream);
        }
    }
}

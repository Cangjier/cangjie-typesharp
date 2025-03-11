using  Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;
public class Response:IDisposable
{
    /// <summary>
    /// 状态代码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应头
    /// </summary>
    public Json Headers { get; set; } = Json.NewObject();

    public MimeTypes? ContentType
    {
        get
        {
            if (!Headers.ContainsKey("Content-Type"))
            {
                return null;
            }
            return MimeTypes.Parse(Headers.Read("Content-Type", string.Empty));
        }
    }

    public MimeTypes? ContentDisposition
    {
        get
        {
            if (!Headers.ContainsKey("Content-Disposition"))
            {
                return null;
            }
            return MimeTypes.Parse(Headers.Read("Content-Disposition", string.Empty));
        }
    }

    public string ContentEncoding
    {
        get => Headers.Read("Content-Encoding", string.Empty);
    }

    public bool ContentIsJson
    {
        get
        {
            var contentType = ContentType;
            if (contentType == null) return false;
            MimeType? multipartMimeType = null;
            foreach (var i in contentType)
            {
                if (i.Master == "application")
                {
                    multipartMimeType = i;
                    break;
                }
            }
            if (multipartMimeType == null) return false;
            if (!multipartMimeType.Types.Contains("json")) return false;
            return true;
        }
    }

    public bool ContentIsForm
    {
        get
        {
            var contentType = ContentType;
            if (contentType == null) return false;
            MimeType? multipartMimeType = null;
            foreach (var i in contentType)
            {
                if (i.Master == "multipart")
                {
                    multipartMimeType = i;
                    break;
                }
            }
            if (multipartMimeType == null) return false;
            if (!multipartMimeType.Types.Contains("form-data") || !multipartMimeType.Map.ContainsKey("boundary")) return false;
            var boundaryValue = multipartMimeType.Map["boundary"];
            string? boundary = null;
            if (boundaryValue is string) boundary = boundaryValue as string;
            if (boundaryValue is Field field) boundary = field.Value;
            if (boundary == null) return false;
            return true;
        }
    }

    /// <summary>
    /// 响应体
    /// </summary>
    public Json Body { get; set; } = Json.Null;

    /// <summary>
    /// 解析响应
    /// </summary>
    /// <param name="request"></param>
    /// <param name="httpResponseMessage"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<Response> Parse(Request request,HttpResponseMessage httpResponseMessage)
    {

        Response result = new();
        result.StatusCode = (int)httpResponseMessage.StatusCode;
        foreach (var item in httpResponseMessage.Headers)
        {
            result.Headers.Set(item.Key, string.Join(",", item.Value));
        }
        foreach(var item in httpResponseMessage.Content.Headers)
        {
            result.Headers.Set(item.Key, string.Join(",", item.Value));
        }
        Stream? contentStream=null;
        List<Stream> toDisposeStreams = [];
        try
        {
            var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            toDisposeStreams.Add(stream);
            if (result.ContentEncoding == string.Empty)
            {
                contentStream = stream;
            }
            else if (result.ContentEncoding == "gzip")
            {
                contentStream= new GZipStream(stream, CompressionMode.Decompress);
                toDisposeStreams.Add(contentStream);
            }
            else if (result.ContentEncoding == "deflate")
            {
                contentStream = new DeflateStream(stream, CompressionMode.Decompress);
                toDisposeStreams.Add(contentStream);
            }
            else if (result.ContentEncoding == "br")
            {
                contentStream = new BrotliStream(stream, CompressionMode.Decompress);
                toDisposeStreams.Add(contentStream);
            }
            else
            {
                throw new Exception("unkown content encoding");
            }
            await result.ProcessStream(request, contentStream);
        }
        finally
        {
            foreach (var item in toDisposeStreams)
            {
                try
                {
                    item.Dispose();
                }
                catch
                {

                }
            }
        }

        return result;
    }

    private async Task ProcessStream(Request request,Stream stream)
    {
        var responseType = request.ResponseType.ToLower();
        if(responseType == "json")
        {
            Body = await Json.ParseAsync(stream);
        }
        else if(responseType == "binary")
        {
            if (
            ContentDisposition is MimeTypes contentDisposition &&
            contentDisposition.Get("attachment") is MimeType attachment &&
            attachment.TryGetString("filename", out var filename))
            {
                string path;
                filename = HttpUtility.UrlDecode(filename);
                var tempDirectory = Path.GetTempPath() + Guid.NewGuid().ToString("N").ToUpperInvariant();
                Directory.CreateDirectory(tempDirectory);
                path = Path.Combine(tempDirectory, filename);
                using var fileStream = File.Create(path);
                await stream.CopyToAsync(fileStream);
                Body = path;
            }
            else
            {
                var path = Path.GetTempFileName();
                using var fileStream = File.Create(path);
                await stream.CopyToAsync(fileStream);
                Body = path;
            }
        }
        else if (responseType == "text")
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var value = UTF8.GetString(memoryStream.ToArray());
            Body = value;
        }
        else if (
            ContentDisposition is MimeTypes contentDisposition &&
            contentDisposition.Get("attachment") is MimeType attachment &&
            attachment.TryGetString("filename", out var filename))
        {
            string path;
            filename = HttpUtility.UrlDecode(filename);
            var tempDirectory = Path.GetTempPath() + Guid.NewGuid().ToString("N").ToUpperInvariant();
            Directory.CreateDirectory(tempDirectory);
            path = Path.Combine(tempDirectory, filename);
            using var fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream);
            Body = path;
        }
        else if (ContentIsJson)
        {
            Body = await Json.ParseAsync(stream);
        }
        else if (ContentIsForm)
        {
            var path = Path.GetTempFileName();
            using var fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream);
            Body = path;
        }
        else
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var value = UTF8.GetString(memoryStream.ToArray());
            if (Json.Validate(value))
            {
                Body = Json.Parse(value);
            }
            else
            {
                Body = value;
            }
        }
    }

    private static UTF8Encoding UTF8 { get; } = new(false);

    public Json ToJson(Treatment treatment)
    {
        Json result = Json.NewObject();
        result.Set("StatusCode", StatusCode);
        result.Set("Headers", Headers);
        result.Set("Body", Body);
        return result;
    }

    //public Json ToNativeTson()
    //{
    //    Json result = Json.NewObject();
    //    result.Set("StatusCode", StatusCode);
    //    result.Set("Headers", Headers);
    //    if(Body is null)
    //    {
    //        result.Set("Body", Json.Null);
    //    }
    //    else
    //    {
    //        result.Set("Body", Body);
    //    }
    //    return result;
    //}

    //public void FromNativeTson(Json value)
    //{
    //    var body = value.Get("Body");
    //    Body = value.Node;
    //}

    public void Dispose()
    {
        Headers = Json.Null;
        Body = Json.Null;
    }
}

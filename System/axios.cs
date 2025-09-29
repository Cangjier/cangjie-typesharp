using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 模拟axios
/// </summary>
public class Axios:IDisposable
{
    public Axios(Context context)
    {
        this.context = context;
        HttpClient.Timeout = TimeSpan.FromDays(8);
    }

    private Context context { get; }

    private HttpClient HttpClient { get; set; } = new HttpClient(new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
    });

    public void setProxy(string proxy)
    {
        HttpClient.Dispose();
        HttpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy(proxy),
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        });
        HttpClient.Timeout = TimeSpan.FromDays(8);
    }

    public void unsetProxy()
    {
        HttpClient.Dispose();
        HttpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        });
        HttpClient.Timeout = TimeSpan.FromDays(8);
    }

    public void setDefaultProxy()
    {
        var gitProxy = Util.GetGitProxy();
        if (string.IsNullOrEmpty(gitProxy)==false)
        {
            setProxy(gitProxy);
            return;
        }
        var systemProxy = Util.GetSystemProxy();
        if (string.IsNullOrEmpty(systemProxy) == false)
        {
            setProxy(systemProxy);
            return;
        }
        var httpProxy = Environment.GetEnvironmentVariable("http_proxy");
        if (string.IsNullOrEmpty(httpProxy) == false)
        {
            setProxy(httpProxy);
            return;
        }

    }

    public async Task<axiosResponse> get(string url)
    {
        return await get(url, null);
    }

    public async Task<axiosResponse> get(string url, axiosConfig? config)
    {
        axiosResponse result = new();
        url = config?.getUrl(url) ?? url;
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        config?.setRequest(request);
        HttpResponseMessage? response;
        if (config?.useDefaultProxy == true)
        {
            response = await HttpClient.SendAsync(request);
            await result.setResponse(response, config,context);
        }
        else
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = string.IsNullOrEmpty(config?.proxy) ? null : new WebProxy(config?.proxy),
            });
            response = await client.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        return result;
    }

    public async Task<axiosResponse> delete(string url, axiosConfig? config)
    {
        axiosResponse result = new();
        url = config?.getUrl(url) ?? url;
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        config?.setRequest(request);
        HttpResponseMessage? response;
        if (config?.useDefaultProxy == true)
        {
            response = await HttpClient.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        else
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = string.IsNullOrEmpty(config?.proxy) ? null : new WebProxy(config?.proxy),
            });
            response = await client.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        return result;
    }

    public async Task<axiosResponse> post(string url, Json data, axiosConfig? config)
    {
        axiosResponse result = new();
        url = config?.getUrl(url) ?? url;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (data.Is<byte[]>())
        {
            request.Content = new ByteArrayContent(data.As<byte[]>());
        }
        else if (data.Is<Stream>())
        {
            request.Content = new StreamContent(data.As<Stream>());
        }
        else
        {
            request.Content = new StringContent(data.ToString(), Util.UTF8, "application/json");
        }
        config?.setRequest(request);
        HttpResponseMessage? response;
        if (config?.useDefaultProxy == true)
        {
            response = await HttpClient.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        else
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = string.IsNullOrEmpty(config?.proxy) ? null : new WebProxy(config?.proxy),
            });
            response = await client.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        return result;
    }

    public async Task<axiosResponse> post(string url, Json data)
    {
        return await post(url, data, null);
    }

    public async Task<axiosResponse> put(string url, Json data, axiosConfig? config)
    {
        axiosResponse result = new();
        url = config?.getUrl(url) ?? url;
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        if (data.Is<byte[]>())
        {
            request.Content = new ByteArrayContent(data.As<byte[]>());
        }
        else
        {
            request.Content = new StringContent(data.ToString(), Util.UTF8, "application/json");
        }
        config?.setRequest(request);
        HttpResponseMessage? response;
        if (config?.useDefaultProxy == true)
        {
            response = await HttpClient.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        else
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = string.IsNullOrEmpty(config?.proxy) ? null : new WebProxy(config?.proxy),
            });
            response = await client.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        return result;
    }

    public async Task<axiosResponse> put(string url, Json data)
    {
        return await put(url, data, null);
    }

    public async Task<axiosResponse> patch(string url, Json data, axiosConfig? config)
    {
        axiosResponse result = new();
        url = config?.getUrl(url) ?? url;
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
        if (data.Is<byte[]>())
        {
            request.Content = new ByteArrayContent(data.As<byte[]>());
        }
        else
        {
            request.Content = new StringContent(data.ToString(), Util.UTF8, "application/json");
        }
        config?.setRequest(request);
        HttpResponseMessage? response;
        if (config?.useDefaultProxy == true)
        {
            response = await HttpClient.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        else
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = string.IsNullOrEmpty(config?.proxy) ? null : new WebProxy(config?.proxy),
            });
            response = await client.SendAsync(request);
            await result.setResponse(response, config, context);
        }
        return result;
    }

    public async Task<axiosResponse> patch(string url, Json data)
    {
        return await patch(url, data, null);
    }

    public async Task<string> download(string url)
    {
        return await download(url, fileName =>
        {
            if (fileName == null)
            {
                return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            }
            else
            {
                return Path.Combine(Path.GetTempPath(), fileName);
            }
        });
    }

    public async Task<string> download(string url,string path)
    {
        return await download(url, fileName => path);
    }

    public async Task<string> download(string url, Func<string?, string> onPath)
    {
        return await download(url, onPath, (current, total) =>
        {

        });
    }

    public async Task<string> download(string url,Func<string?, string> onPath,Action<long, long?> onProgress)
    {
        var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        var path = onPath(response.Content.Headers.ContentDisposition?.FileName);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        Stream decompressionStream = responseStream;

        // Determine if the content is compressed and wrap the stream accordingly
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            decompressionStream = new GZipStream(responseStream, CompressionMode.Decompress);
        }
        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        {
            decompressionStream = new DeflateStream(responseStream, CompressionMode.Decompress);
        }
        else if (response.Content.Headers.ContentEncoding.Contains("br"))
        {
            decompressionStream = new BrotliStream(responseStream, CompressionMode.Decompress);
        }

        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        const int bufferSize = 8192; // 8KB buffer size
        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;

        while (true)
        {
            int bytesRead = await decompressionStream.ReadAsync(buffer, 0, bufferSize);
            if (bytesRead == 0)
            {
                break; // No more data to read
            }

            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;

            // Invoke the progress callback
            onProgress(totalBytesRead, contentLength);
        }

        return path;
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
        HttpClient = null!;
    }
}

public class axiosResponse
{
    /// <summary>
    /// 返回的data
    /// </summary>
    public object? data { get; set; }

    public Dictionary<string, string> headers { get; } = [];

    public int status { get; set; }

    public string statusText { get; set; } = "";

    public async Task setResponse(HttpResponseMessage response,axiosConfig? config,Context context)
    {
        status = (int)response.StatusCode;
        statusText = response.ReasonPhrase ?? string.Empty;
        foreach (var item in response.Headers)
        {
            headers.Add(item.Key, item.Value.Join(","));
        }
        // 根据 headers 中的 Content-Type 判断返回的数据类型
        if (response.Content != null)
        {
            foreach (var item in response.Content.Headers)
            {
                headers.Add(item.Key, item.Value.Join(","));
            }
            if (config == null || config.responseType == "")
            {
                var content = await response.GetResponseContentAsync();
                try
                {
                    if (headers.ContainsKey("Content-Type"))
                    {
                        var contentType = headers["Content-Type"];
                        if (contentType.Contains("application/json"))
                        {
                            data = Json.Parse(content);
                        }
                        else
                        {
                            data = content;
                        }
                    }
                    else
                    {
                        data = content;
                    }
                }
                catch
                {
                    data = content;
                    context.console.error(ToString());
                }
            }
            else if (config.responseType == "json")
            {
                var content = await response.GetResponseContentAsync();
                try
                {
                    data = Json.Parse(content);
                }
                catch
                {
                    data = content;
                    context.console.error(ToString());
                    throw;
                }
            }
            else if (config.responseType == "text")
            {
                data = await response.GetResponseContentAsync();
            }
            else if (config.responseType == "arraybuffer")
            {
                data = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                data = await response.Content.ReadAsByteArrayAsync();
            }
        }
    }

    public override string ToString()
    {
        return new Json(this).ToString();
    }
}

public class axiosConfig
{
    public static implicit operator axiosConfig(Json target)
    {
        var result = new axiosConfig();
        result.debug = target.Read("debug", false);
        result.proxy = target.Read("proxy", "");
        result.useDefaultProxy = target.Read("useDefaultProxy", true);
        if (target.ContainsKey("headers"))
        {
            foreach (var item in target.Get("headers").GetObjectEnumerable())
            {
                result.headers.Add(item.Key, item.Value.AsString);
            }
        }
        if (target.ContainsKey("responseType"))
        {
            result.responseType = target.Get("responseType").AsString;
        }
        if (target.ContainsKey("params"))
        {
            foreach (var item in target.Get("params").GetObjectEnumerable())
            {
                result.@params.Add(item.Key, item.Value.AsString);
            }
        }
        return result;
    }

    public Dictionary<string, string> headers = [];

    public Dictionary<string, string> @params = [];

    public string responseType = "";

    public bool debug = false;

    public string proxy { get; set; } = "";

    public bool useDefaultProxy = true;

    public void setRequest(HttpRequestMessage request)
    {
        foreach (var (key, value) in headers)
        {
            if (key.Contains("Content") == false)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
            else
            {
                request.Content?.Headers.TryAddWithoutValidation(key, value);
            }
        }
    }

    public string getUrl(string url)
    {
        if (@params.Count == 0)
        {
            return url;
        }
        var uriBuilder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in @params)
        {
            query[param.Key] = param.Value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public override string ToString()
    {
        return new Json(this).ToString();
    }
}

internal static class axiosUtils
{
    public static async Task<string> GetResponseContentAsync(this HttpResponseMessage response)
    {
        // 检查是否包含 Content-Encoding 头
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
        else if (response.Content.Headers.ContentEncoding.Contains("br"))
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var decompressionStream = new BrotliStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
        else
        {
            // 如果没有 Content-Encoding，按默认方式读取
            return await response.Content.ReadAsStringAsync();
        }
    }
}

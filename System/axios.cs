﻿using System.Net;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 模拟axios
/// </summary>
public class axios
{
    private static HttpClient HttpClient { get; set; } = new HttpClient();

    public static void setProxy(string proxy)
    {
        HttpClient.Dispose();
        HttpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy(proxy)
        });
    }

    public static void unsetProxy()
    {
        HttpClient.Dispose();
        HttpClient = new HttpClient();
    }

    public static async Task<axiosResponse> get(string url)
    {
        return await get(url, null);
    }

    public static async Task<axiosResponse> get(string url, axiosConfig? config)
    {
        axiosResponse result = new();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (config != null)
        {
            foreach (var (key, value) in config.headers)
            {
                if (key.Contains("Content") == false)
                {
                    request.Headers.Add(key, value);
                }
                else
                {
                    request.Content?.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }
        var response = await HttpClient.SendAsync(request);
        result.status = (int)response.StatusCode;
        result.statusText = response.ReasonPhrase ?? string.Empty;
        foreach (var item in response.Headers)
        {
            result.headers.Add(item.Key, item.Value.Join(","));
        }
        // 根据 headers 中的 Content-Type 判断返回的数据类型
        if (response.Content != null)
        {
            foreach (var item in response.Content.Headers)
            {
                result.headers.Add(item.Key, item.Value.Join(","));
            }
            if (config == null)
            {
                result.data = Json.Parse(await response.Content.ReadAsStringAsync());
            }
            else if (config.responseType == "json")
            {
                result.data = Json.Parse(await response.Content.ReadAsStringAsync());
            }
            else if (config.responseType == "text")
            {
                result.data = await response.Content.ReadAsStringAsync();
            }
            else if (config.responseType == "arraybuffer")
            {
                result.data = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                result.data = await response.Content.ReadAsByteArrayAsync();
            }
        }
        return result;
    }
    
    public static async Task<axiosResponse> post(string url, Json data, axiosConfig? config)
    {
        axiosResponse result = new();
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (data.Is<byte[]>())
        {
            request.Content = new ByteArrayContent(data.As<byte[]>());
        }
        else
        {
            request.Content = new StringContent(data.ToString(), Util.UTF8, "application/json");
        }
        if (config != null)
        {
            foreach (var (key, value) in config.headers)
            {
                if (key.Contains("Content") == false)
                {
                    request.Headers.Add(key, value);
                }
                else
                {
                    request.Content?.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }
        var response = await HttpClient.SendAsync(request);
        result.status = (int)response.StatusCode;
        result.statusText = response.ReasonPhrase ?? string.Empty;
        foreach (var item in response.Headers)
        {
            result.headers.Add(item.Key, item.Value.Join(","));
        }
        // 根据 headers 中的 Content-Type 判断返回的数据类型
        if (response.Content != null)
        {
            foreach (var item in response.Content.Headers)
            {
                result.headers.Add(item.Key, item.Value.Join(","));
            }
            if (config == null)
            {
                result.data = Json.Parse(await response.Content.ReadAsStringAsync());
            }
            else if (config.responseType == "json")
            {
                result.data = Json.Parse(await response.Content.ReadAsStringAsync());
            }
            else if (config.responseType == "arraybuffer")
            {
                result.data = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                result.data = await response.Content.ReadAsByteArrayAsync();
            }
        }
        return result;
    }

    public static async Task<axiosResponse> post(string url, Json data)
    {
        return await post(url, data, null);
    }

    public static async Task download(string url,string path)
    {
        var response = await HttpClient.GetAsync(url);
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream);
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
}

public class axiosConfig
{
    public static implicit operator axiosConfig(Json target)
    {
        var result = new axiosConfig();
        if(target.ContainsKey("headers"))
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
        return result;
    }

    public Dictionary<string, string> headers { get; set; } = [];

    public string responseType = "json";
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

public class netUtils
{
    private static async Task<int> pingAsync(HttpClient httpClient, string url, pingConfig? config = null)
    {
        int retryCount = config?.count ?? 1;
        int timeout = config?.timeout ?? 5000;
        int[] result = new int[retryCount];
        List<Task> tasks = [];
        for (int i = 0; i < retryCount; i++)
        {
            int innerI = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    using var cancellationTokenSource = new CancellationTokenSource(timeout);
                    using var response = await httpClient.GetAsync(url, cancellationTokenSource.Token);
                    _ = await response.Content.ReadAsByteArrayAsync();
                    stopwatch.Stop();
                    result[innerI] = (int)stopwatch.ElapsedMilliseconds;
                }
                catch
                {
                    result[innerI] = -1;
                }
            }));
        }
        await Task.WhenAll(tasks);
        // 如果全是-1，则返回-1
        if (result.All(x => x == -1))
        {
            return -1;
        }
        else
        {
            // 否则返回非-1的平均值
            return (int)result.Where(x => x != -1).Average();
        }
    }

    public static async Task<int> pingAsync(string url, pingConfig? config = null)
    {
        WebProxy? proxy = null;
        if (config?.proxy != null)
        {
            var uri = new Uri(config.proxy);
            proxy = new WebProxy { Address = uri };
        }
        // 创建HttpClientHandler并设置代理1  
        using var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true,
        };

        // 创建HttpClient并使用代理1  
        using var httpClient = new HttpClient(httpClientHandler);
        return await pingAsync(httpClient, url, config);
    }

    public static async Task<int[]> pingsAsync(string[] urls, pingConfig[] configs)
    {
        return await pingsWithConfigsAsync(urls, configs);
    }

    public static async Task<int> pingsWithConfigAsync(string[] urls, pingConfig? config = null)
    {
        WebProxy? proxy = null;
        if (config?.proxy != null)
        {
            var uri = new Uri(config.proxy);
            proxy = new WebProxy { Address = uri };
        }
        // 创建HttpClientHandler并设置代理1  
        using var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true,
        };

        // 创建HttpClient并使用代理1  
        using var httpClient = new HttpClient(httpClientHandler);

        List<Task<int>> tasks = [];
        foreach (var url in urls)
        {
            tasks.Add(pingAsync(httpClient, url, config));
        }
        var pingsResult = await Task.WhenAll(tasks);
        // 如果全是-1，则返回-1
        if (pingsResult.All(x => x == -1))
        {
            return -1;
        }
        else
        {
            // 否则返回非-1的平均值
            return (int)pingsResult.Where(x => x != -1).Average();
        }
    }

    public static async Task<int[]> pingWithConfigsAsync(string url, pingConfig[] configs)
    {
        List<Task<int>> tasks = [];
        foreach (var config in configs)
        {
            tasks.Add(pingAsync(url, config));
        }
        return await Task.WhenAll(tasks);
    }

    public static async Task<int[]> pingsWithConfigsAsync(string[] urls, pingConfig[] configs)
    {
        List<Task<int>> tasks = [];
        foreach (var config in configs)
        {
            tasks.Add(pingsWithConfigAsync(urls, config));
        }
        return await Task.WhenAll(tasks);
    }
}

public class pingConfig
{
    private Json Target { get; }

    public pingConfig()
    {
        Target = Json.NewObject();
    }

    public pingConfig(Json target)
    {
        Target = target;
    }

    public static implicit operator pingConfig(Json target)
    {
        return new pingConfig(target);
    }

    public int timeout
    {
        get => Target.Read("timeout", 5000);
        set => Target.Set("timeout", value);
    }

    public int count
    {
        get => Target.Read("count", 4);
        set => Target.Set("count", value);
    }

    public string proxy
    {
        get => Target.Read("proxy", string.Empty);
        set => Target.Set("proxy", value);
    }


}
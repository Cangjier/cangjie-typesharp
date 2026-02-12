using TidyHPC.LiteHttpServer;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using Cangjie.TypeSharp.Server.TaskQueues.Agents;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;
using System.Net.WebSockets;

namespace Cangjie.TypeSharp.Server.TaskQueues;

/// <summary>
/// 共享服务器
/// </summary>
public class ShareServer
{
    /// <summary>
    /// 共享服务
    /// </summary>
    public ShareServer(TaskService taskService)
    {
        TaskService = taskService;
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    private string _UrlPrefix = string.Empty;

    /// <summary>
    /// Url前缀
    /// </summary>
    public string UrlPrefix
    {
        get => _UrlPrefix;
        set
        {
            if (value == string.Empty)
            {
                _UrlPrefix = string.Empty;
                return;
            }
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                _UrlPrefix = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            }
            else
            {
                throw new Exception("Invalid UrlPrefix");
            }
        }
    }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = false;

    private static HttpClient HttpClient { get; } = new();

    /// <summary>
    /// 代理人连接
    /// </summary>
    private WebsocketClient WebsocketClient { get; set; } = new();

    /// <summary>
    /// 代理人Id
    /// </summary>
    public Guid AgentId { get; set; } = Guid.Empty;

    /// <summary>
    /// 启动共享服务相关连接
    /// </summary>
    /// <param name="urlRouter"></param>
    /// <returns></returns>
    public async Task Start(UrlRouter urlRouter)
    {
        _ = Task.Run(StartUpdatePerformance);
        while (true)
        {
            if (Enabled == false)
            {
                await Task.Delay(10000);
                continue;
            }
            try
            {
                if (Uri.TryCreate(UrlPrefix, new(), out var uri))
                {
                    await WebsocketClient.Connect($"ws://{uri.Host}:{uri.Port}/", new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json" }
                    });
                    Logger.Info($"Connect to {UrlPrefix}");
                }
                else
                {
                    throw new Exception("Invalid UrlPrefix");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await Task.Delay(10000);
                WebsocketClient.Dispose();
                WebsocketClient = new WebsocketClient();
                continue;
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100);
                    Json plugins = Json.NewArray();
                    TaskService.PluginCollection.GetPlugins(item => plugins.Add(item.Target.Clone()));
                    await Register(plugins);
                }
                catch (Exception e)
                {
                    Logger.Error("Register agent failed", e);
                }
            });
            try
            {
                await urlRouter.Listen(WebsocketClient, CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            await Task.Delay(10000);
            WebsocketClient.Dispose();
            WebsocketClient = new WebsocketClient();
        }
    }

    /// <summary>
    /// 启动性能信息更新
    /// </summary>
    /// <returns></returns>
    private async Task StartUpdatePerformance()
    {
        while (true)
        {
            if (Enabled && AgentId != Guid.Empty && WebsocketClient.State == WebSocketState.Open)
            {
                try
                {
                    await UpdatePerformance(AgentId, PerformanceInterface.GetCurrent());
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            await Task.Delay(10000);
        }
    }

    /// <summary>
    /// 向共享服务注册
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pluginsArray"></param>
    /// <returns></returns>
    private async Task<Guid> Register(Guid id, Json pluginsArray)
    {
        if (WebsocketClient.State != WebSocketState.Open)
        {
            throw new Exception("WebsocketClient is not open");
        }
        Json body = Json.NewObject();
        var websocket_session_id = Guid.NewGuid();
        body["url"] = Apis.V2.Agents.Server.Register.url;
        body["response"] = Apis.V2.Response.url;
        body["websocket_session_id"] = websocket_session_id;
        body["agentId"] = id;
        body["plugins"] = pluginsArray.Clone();
        var task = TaskService.TaskCompletion.Add(websocket_session_id);
        await WebsocketClient.SendMessage(body.ToString());
        var result = await task.Task.WaitAsync(TimeSpan.FromSeconds(30));
        if (result is NetMessageInterface msg)
        {
            if (msg.success)
            {
                return msg.data.AsGuid;
            }
            else
            {
                throw new Exception(msg.message);
            }
        }
        else
        {
            throw new Exception("Register agent failed");
        }
    }

    private async Task UpdatePerformance(Guid id, Json performance)
    {
        if (WebsocketClient.State != WebSocketState.Open)
        {
            throw new Exception("WebsocketClient is not open");
        }
        Json body = Json.NewObject();
        var websocket_session_id = Guid.NewGuid();
        body["url"] = Apis.V2.Agents.Server.UpdatePerformance.url;
        body["response"] = Apis.V2.Response.url;
        body["websocket_session_id"] = websocket_session_id;
        body["agentId"] = id;
        body["hostName"] = Environment.MachineName;
        body["performance"] = performance.Clone();
        var task = TaskService.TaskCompletion.Add(websocket_session_id);
        await WebsocketClient.SendMessage(body.ToString());
        var result = await task.Task.WaitAsync(TimeSpan.FromSeconds(30));
        if (result is NetMessageInterface msg)
        {
            if (msg.success)
            {
                return;
            }
            else
            {
                throw new Exception(msg.message);
            }
        }
        else
        {
            throw new Exception("Update performance failed");
        }
    }

    private async Task UpdatePlugins(Guid id, Json plugins)
    {
        if (WebsocketClient.State != WebSocketState.Open)
        {
            throw new Exception("WebsocketClient is not open");
        }
        Json body = Json.NewObject();
        var websocket_session_id = Guid.NewGuid();
        body["url"] = Apis.V2.Agents.Server.UpdatePlugins.url;
        body["response"] = Apis.V2.Response.url;
        body["websocket_session_id"] = websocket_session_id;
        body["agentId"] = id;
        body["hostName"] = Environment.MachineName;
        body["plugins"] = plugins.Clone();
        var task = TaskService.TaskCompletion.Add(websocket_session_id);
        await WebsocketClient.SendMessage(body.ToString());
        var result = await task.Task.WaitAsync(TimeSpan.FromSeconds(30));
        if (result is NetMessageInterface msg)
        {
            if (msg.success)
            {
                return;
            }
            else
            {
                throw new Exception(msg.message);
            }
        }
        else
        {
            throw new Exception("Update plugins failed");
        }
    }

    /// <summary>
    /// 注册插件
    /// </summary>
    /// <param name="pluginsArray"></param>
    /// <returns></returns>
    public async Task Register(Json pluginsArray)
    {
        if (Enabled && WebsocketClient.State != WebSocketState.None)
        {
            try
            {
                AgentId = await Register(Guid.NewGuid(), pluginsArray);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }

    /// <summary>
    /// 尝试更新插件
    /// </summary>
    /// <param name="pluginsArray"></param>
    /// <returns></returns>
    public async Task Update(Json pluginsArray)
    {
        if (Enabled && WebsocketClient.State != WebSocketState.None)
        {
            try
            {
                await UpdatePlugins(AgentId, pluginsArray);
            }
            catch
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 运行任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task Run(TaskInterface task)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, Apis.V2.Tasks.Run.WithPrefix(UrlPrefix));
        httpRequestMessage.Content = new StringContent(task.ToString(), Util.UTF8, "application/json");
        var response = await HttpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStreamAsync();
            NetMessageInterface responseMessage = await Json.ParseAsync(responseContent);
            if (responseMessage.success)
            {
                TaskInterface responseTask = responseMessage.data;
                task.Output = responseTask.Output.Clone();
                task.Trace.Update(task.Trace);
            }
            else
            {
                task.Trace.Error(responseMessage.message);
                task.Trace.Update(responseMessage.Trace);
            }
        }
        else
        {
            task.Trace.Error($"Run task failed, status code: {response.StatusCode}");
        }
    }
}

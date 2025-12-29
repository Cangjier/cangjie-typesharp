using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server;
using Cangjie.TypeSharp.Server.TaskQueues;
using Cangjie.TypeSharp.Server.WebService;
namespace Cangjie.TypeSharp.Server;
/// <summary>
/// 代理应用程序配置
/// </summary>
public class AgentApplicationConfig
{
    /// <summary>
    /// 插件目录
    /// </summary>
    public string? PluginsDirectory { get; set; }
    /// <summary>
    /// 共享服务器URL前缀
    /// </summary>
    public string ShareServerUrlPrefix { get; set; } = string.Empty;
}

/// <summary>
/// 代理应用程序
/// </summary>
public class AgentApplication
{

    /// <summary>
    /// 构造函数
    /// </summary>
    public AgentApplication()
    {
        TaskService = new TaskService();
        UrlRouter = new();
        AgentClientWebService = new(TaskService);
        CommonWebService = new(TaskService);
        Logger.Info("AgentApplication constructor");
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; private set; }

    private CommonWebService CommonWebService { get; }

    private AgentWebService.Client AgentClientWebService { get; }

    /// <summary>
    /// Url路由
    /// </summary>
    private UrlRouter UrlRouter { get; }

    /// <summary>
    /// 启动应用程序
    /// </summary>
    public async Task Start(AgentApplicationConfig config)
    {
        UrlResponse.DefaultContentEncoding = "gzip";
        Dictionary<string, string> copyKeys = new()
        {
            ["websocket_session_id"] = "websocket_session_id",
            ["response"] = "url"
        };
        UrlRouter.Events.OnResponseJsonGenerated = async (session, responseJson) =>
        {
            if (session.Response is IWebsocketResponse)
            {
                var requestJson = await session.Cache.GetRequstBodyJson();
                if (requestJson.IsObject)
                {
                    foreach (var pair in copyKeys)
                    {
                        if (requestJson.ContainsKey(pair.Key))
                        {
                            responseJson.Set(pair.Value, requestJson[pair.Key].Clone());
                        }
                    }
                }
            }
        };
        Logger.Info("Start AgentApplication ...");
        if (config.PluginsDirectory != null)
        {
            var fullPath = config.PluginsDirectory;
            if (Path.IsPathRooted(fullPath) == false)
            {
                fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", fullPath);
            }
            Util.CreateDirectory(fullPath);
            Logger.Info($"PluginDirectory {fullPath}");
            TaskService.PluginCollection.SetPluginDirectory(fullPath, true);
        }
        Logger.Info("Enable ShareServer");
        TaskService.ShareServer.Enabled = true;
        Logger.Info($"ShareServerUrlPrefix {config.ShareServerUrlPrefix}");
        TaskService.ShareServer.UrlPrefix = config.ShareServerUrlPrefix;
        Logger.Info("Register UrlRouter");
        UrlRouter.Register([Apis.V2.Response.pattern], CommonWebService.Response);
        UrlRouter.Register([Apis.V2.Agents.Client.Run.pattern], AgentClientWebService.Run);
        Logger.Info("Start ShareServer");
        await TaskService.ShareServer.Start(UrlRouter);
    }
}
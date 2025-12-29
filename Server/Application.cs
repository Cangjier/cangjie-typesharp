using System.Net.Http.Headers;
using TidyHPC.ASP.LiteKestrelServers;
using TidyHPC.Common;
using TidyHPC.Extensions;
using TidyHPC.LiteDB;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server.TaskQueues;
using Cangjie.TypeSharp.Server.WebService;
using TidyHPC.LiteJson;


namespace Cangjie.TypeSharp.Server;

/// <summary>
/// 应用程序配置
/// </summary>
public class ApplicationConfig()
{
    /// <summary>
    /// Http服务器地址
    /// </summary>
    public int[] ServerPorts { get; set; } = [];

    /// <summary>
    /// 是否启用所有网络设备
    /// </summary>
    public bool EnableAnyIP { get; set; } = true;

    /// <summary>
    /// 是否启用插件
    /// </summary>
    public bool EnablePlugins { get; set; } = true;

    /// <summary>
    /// 插件目录
    /// </summary>
    public string? PluginsDirectory { get; set; }

    /// <summary>
    /// 启用监听插件目录
    /// </summary>
    public bool EnableListenPluginsDirectory { get; set; } = false;

    /// <summary>
    /// 启用共享服务器
    /// </summary>
    public bool EnableShareServer { get; set; } = false;

    /// <summary>
    /// 共享服务器地址前缀
    /// </summary>
    public string ShareServerUrlPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 静态资源路径
    /// </summary>
    public string? StaticResourcePath { get; set; } = null;

    /// <summary>
    /// ssl证书路径
    /// </summary>
    public string SSLCertificatePath { get; set; } = string.Empty;

    /// <summary>
    /// ssl证书密码
    /// </summary>
    public string SSLCertificatePassword { get; set; } = string.Empty;

    /// <summary>
    /// 拓展API接口的路由正则匹配，默认支持/api/v\d*/的接口
    /// </summary>
    public string? ExternalAPIRegex { get; set; } = null;
}

/// <summary>
/// 集成应用
/// </summary>
public class Application
{
    /// <summary>
    /// 应用程序
    /// </summary>
    public Application()
    {
        TaskService = new();
        HttpServer = new();
        UrlRouter = new();

        CommonWebService = new(TaskService);
        TaskWebService = new(TaskService);
        AgentServerWebService = new(TaskService);
        AgentClientWebService = new(TaskService);
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    private LiteKestrelServer HttpServer { get; }

    /// <summary>
    /// Url路由
    /// </summary>
    private UrlRouter UrlRouter { get; }

    private CommonWebService CommonWebService { get; }

    private TaskWebService TaskWebService { get; }

    private AgentWebService.Server AgentServerWebService { get; }

    private AgentWebService.Client AgentClientWebService { get; }

    /// <summary>
    /// 当配置完成时
    /// </summary>
    public TaskCompletionSource OnConfigCompleted { get; } = new();

    /// <summary>
    /// 注册API接口
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="delegate"></param>
    public void Register(string pattern, Delegate @delegate)
    {
        UrlRouter.Register([pattern], @delegate);
    }

    /// <summary>
    /// 启动应用程序
    /// </summary>
    /// <param name="config"></param>
    public async Task Start(ApplicationConfig config)
    {

        //配置TaskService
        if (config.PluginsDirectory != null)
        {
            var fullPath = config.PluginsDirectory;
            if (Path.IsPathRooted(fullPath) == false)
            {
                fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", fullPath);
            }
            Util.CreateDirectory(fullPath);
            Logger.Info($"PluginDirectory {fullPath}");
            TaskService.PluginCollection.SetPluginDirectory(fullPath, config.EnablePlugins);
        }
        Logger.Info($"Enable ShareServer {config.EnableShareServer}");
        TaskService.ShareServer.Enabled = config.EnableShareServer;
        Logger.Info($"ShareServerUrlPrefix {config.ShareServerUrlPrefix}");
        TaskService.ShareServer.UrlPrefix = config.ShareServerUrlPrefix;
        //配置Http服务器
        HttpServer.EnableAnyIP = config.EnableAnyIP;
        HttpServer.ListenPorts.AddRange(config.ServerPorts);
        var enableSSL = File.Exists(config.SSLCertificatePath);
        Logger.Info("Server starting...");
        for (int i = 0; i < config.ServerPorts.Length; i++)
        {
            Logger.Info($"Server started at {i + 1,3}/{config.ServerPorts.Length} {config.ServerPorts[i]}");
            if (TaskService.CurrentServerUrlPrefix == string.Empty)
            {
                if (enableSSL)
                {
                    TaskService.CurrentServerUrlPrefix = $"https://127.0.0.1:{config.ServerPorts[i]}";
                }
                else
                {
                    TaskService.CurrentServerUrlPrefix = $"http://127.0.0.1:{config.ServerPorts[i]}";
                }
            }
        }
        if (enableSSL)
        {
            HttpServer.X509Certificate2 = new(config.SSLCertificatePath, config.SSLCertificatePassword);
        }
        //配置默认编码
        UrlResponse.DefaultContentEncoding = "gzip";
        //配置路由
        UrlRouter.Events.HandleNoRoute = async (url, session) =>
        {
            await Task.CompletedTask;
            session.Complete(() =>
            {
                Logger.InfoParameter("404", url);
                session.Response.StatusCode = 404;
                session.Response.Headers.SetHeader("Content-Type", "text/html");
                session.Response.Body.Write(Util.UTF8.GetBytes($"""
            <html>
            <head>
            <title>404 Not Found</title>
            </head>
            <body>
            <h1>404 Not Found</h1>
            <p>The requested URL was not found on this server.</p>
            <p>URL: {url}</p>
            </body>
            </html>
            """));
            });
        };
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
        //添加静态资源响应
        string staticResourcePath = config.StaticResourcePath ?? Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build");
        string apiRegex = config.ExternalAPIRegex == null ?
            @"/api/v\d*/" :
            @$"(/api/v\d*/|{config.ExternalAPIRegex})";
        UrlRouter.Register([@$"^(?!{apiRegex})(?<filePath>.*)$"], async (Session session, string filePath) =>
        {
            await Task.CompletedTask;
            session.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(30)
            };
            if (filePath.Contains('.'))
            {
                string fullPath = Path.Combine(staticResourcePath, filePath.TrimStart('/'));
                return new DetectFile(fullPath, filePath);
            }
            else
            {
                string fullPath = Path.Combine(staticResourcePath, filePath.TrimStart('/'));
                while (true)
                {
                    var indexFilePath = Path.Combine(fullPath, "index.html");
                    if (File.Exists(indexFilePath))
                    {
                        return new DetectFile(indexFilePath, filePath);
                    }
                    if (fullPath == staticResourcePath)
                    {
                        break;
                    }
                    fullPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
                }
                return new DetectFile(Path.Combine(staticResourcePath, "index.html"), filePath);
            }
        });
        //对所有请求添加跨源，对Options请求直接过滤
        UrlRouter.Filter.Register(0, [".*"], async (Session session) =>
        {
            await Task.CompletedTask;
            session.Response.Headers.SetHeader("Access-Control-Allow-Origin", "*");
            session.Response.Headers.SetHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            session.Response.Headers.SetHeader("Access-Control-Allow-Headers", "*");
            session.Response.Headers.SetHeader("Access-Control-Allow-Credentials", "true");

            if (session.Request.Method == UrlMethods.HTTP_OPTIONS)
            {
                return false;
            }
            return true;
        });
        //添加CommonService
        UrlRouter.Register([Apis.V2.Response.pattern], CommonWebService.Response);


        #region TaskService
        //添加TaskWebService
        UrlRouter.Register([Apis.V2.Tasks.Run.pattern], TaskWebService.Run);
        UrlRouter.Register([Apis.V2.Tasks.RunAsync.pattern], TaskWebService.RunAsync);
        UrlRouter.Register([Apis.V2.Tasks.Query.pattern], TaskWebService.Query);
        UrlRouter.Register([Apis.V2.Tasks.PluginRunAsync.pattern], TaskWebService.PluginRunAsync);
        UrlRouter.Register([Apis.V2.Tasks.SubscribeProgress.pattern], TaskWebService.SubscribeProgress);
        UrlRouter.Register([Apis.V2.Tasks.UpdateProgress.pattern], TaskWebService.UpdateProgress);
        UrlRouter.Register([Apis.V2.Agents.Server.Register.pattern], AgentServerWebService.Register);
        UrlRouter.Register([Apis.V2.Agents.Server.UpdatePerformance.pattern], AgentServerWebService.UpdatePerformance);
        UrlRouter.Register([Apis.V2.Agents.Server.UpdatePlugins.pattern], AgentServerWebService.UpdatePlugins);
        UrlRouter.Register([Apis.V2.Agents.Server.Get.pattern], AgentServerWebService.Get);
        UrlRouter.Register([Apis.V2.Agents.Server.InstallPackage.pattern], AgentServerWebService.InstallPackage);
        UrlRouter.Register([Apis.V2.Agents.Client.Run.pattern], AgentClientWebService.Run);
        #endregion

        bool isFirstLoadedPlugins = false;
        TaskService.PluginCollection.OnLoadedPlugins += () =>
        {
            if (isFirstLoadedPlugins)
            {
                return;
            }
            isFirstLoadedPlugins = true;
            _ = Task.Run(async () =>
            {
                var context = new Cangjie.TypeSharp.System.Context();
                context.Logger = Logger.LoggerFile;
                var scriptContext = context.context;
                scriptContext["server"] = new Json(new Cangjie.TypeSharp.System.Server(this));
                var disposes = await TaskService.PluginCollection.RunTypeSharpService(context);
                // 不对资源进行释放，保持长驻内存
                await Task.Delay(Timeout.Infinite);
                disposes.Dispose();
            });
        };

        var mainTask = HttpServer.Start();
        _ = TaskService.ShareServer.Start(UrlRouter);
        _ = UrlRouter.Listen(HttpServer, CancellationToken.None);
        OnConfigCompleted.SetResult();
        await mainTask;
    }

}

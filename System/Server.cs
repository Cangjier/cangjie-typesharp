using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 内置服务器
/// </summary>
public class Server
{
    public Server()
    {
        Application = new Application();
    }
    public Server(Application application)
    {
        Application = application;
    }
    public Application Application { get; }

    public ApplicationConfig ApplicationConfig { get; } = new();

    public TaskCompletionSource onConfigCompleted => Application.OnConfigCompleted;

    public async Task start(Json port)
    {
        List<int> ports = [];
        if (port.IsArray)
        {
            foreach (var item in port.GetArrayEnumerable())
            {
                ports.Add(item.ToInt32);
            }
        }
        else
        {
            ports.Add(port.ToInt32);
        }
        ApplicationConfig.ServerPorts = ports.ToArray();
        await Application.Start(ApplicationConfig);
    }

    /// <summary>
    /// 使用路由
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="delegate"></param>
    public void use(string pattern, Delegate @delegate)
    {
        Application.Register(pattern, @delegate);
    }

    public void useStatic(string directory)
    {
        ApplicationConfig.StaticResourcePath = directory;
    }

    public void usePlugins(string directory, bool enable)
    {
        ApplicationConfig.PluginsDirectory = directory;
        ApplicationConfig.EnablePlugins = enable;
    }
}
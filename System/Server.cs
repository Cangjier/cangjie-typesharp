using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server;
using TidyHPC.ASP.LiteKestrelServers;
using System.Security.Cryptography.X509Certificates;

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

    public async Task start(Json ports)
    {
        listen(ports);
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

    public void listen(Json ports)
    {
        if (ports.IsArray)
        {
            foreach (var item in ports.GetArrayEnumerable())
            {
                int port = item.ToInt32;
                if (ApplicationConfig.ServerPorts.ContainsKey(port))
                {
                    continue;
                }
                ApplicationConfig.ServerPorts.Add(port, new LiteKestrelServer.PortConfig { Port = port });
            }
        }
        else
        {
            int port = ports.ToInt32;
            if (ApplicationConfig.ServerPorts.ContainsKey(port))
            {
                return;
            }
            ApplicationConfig.ServerPorts.Add(port, new LiteKestrelServer.PortConfig { Port = port });
        }
    }

    public void useSSL(int port, string certificatePath, string certificateKeyPath)
    {
        if (ApplicationConfig.ServerPorts.TryGetValue(port, out var portConfig) == false)
        {
            portConfig = new LiteKestrelServer.PortConfig { Port = port };
            ApplicationConfig.ServerPorts.Add(port, portConfig);
        }
        portConfig.X509Certificate2 = X509Certificate2.CreateFromPemFile(certificatePath, certificateKeyPath);
    }

    public void useHttpsRedirect()
    {
        ApplicationConfig.EnableHttpsRedirect = true;
    }


}
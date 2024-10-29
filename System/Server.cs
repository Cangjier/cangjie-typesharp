using VizGroup.V1;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 内置服务器
/// </summary>
public class Server
{
    private Application Application { get; } = new();

    private ApplicationConfig ApplicationConfig { get; } = new();

    public async Task start(int port)
    {
        ApplicationConfig.ServerPorts = [port];
        await Application.Start(ApplicationConfig);
    }

    /// <summary>
    /// 使用路由
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="delegate"></param>
    public void use(string pattern,Delegate @delegate)
    {
        Application.Register(pattern,@delegate);
    }

    public void useStatic(string directory)
    {
        ApplicationConfig.StaticResourcePath = directory;
    }
}

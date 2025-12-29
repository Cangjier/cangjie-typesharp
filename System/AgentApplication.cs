using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;
/// <summary>
/// AgentApplication class
/// </summary>
public class AgentApplication
{
    public AgentApplication()
    {
        Application = new Cangjie.TypeSharp.Server.AgentApplication();
        Config = new Cangjie.TypeSharp.Server.AgentApplicationConfig();
    }

    public Cangjie.TypeSharp.Server.AgentApplication Application { get; }

    public Cangjie.TypeSharp.Server.AgentApplicationConfig Config { get; }

    /// <summary>
    /// use plugin
    /// </summary>
    /// <param name="pluginDirectory"></param>
    public void use(string pluginDirectory)
    {
        Config.PluginsDirectory = pluginDirectory;
    }

    /// <summary>
    /// start
    /// </summary>
    /// <param name="shareServerUrlPrefix"></param>
    /// <returns></returns>
    public async Task start(string shareServerUrlPrefix)
    {
        Config.ShareServerUrlPrefix = shareServerUrlPrefix;
        await Application.Start(Config);
    }
}
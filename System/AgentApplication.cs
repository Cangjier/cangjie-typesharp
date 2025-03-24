using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;
/// <summary>
/// AgentApplication class
/// </summary>
public class AgentApplication
{
    public AgentApplication()
    {
        Application = new VizGroup.V1.AgentApplication();
        Config = new VizGroup.V1.AgentApplicationConfig();
    }

    public VizGroup.V1.AgentApplication Application { get; }

    public VizGroup.V1.AgentApplicationConfig Config { get; }

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
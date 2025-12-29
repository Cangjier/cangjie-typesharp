using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Semaphores;
using Cangjie.TypeSharp.Server.TaskQueues.Agents;
using Cangjie.TypeSharp.Server.TaskQueues.Plugins;
using Cangjie.TypeSharp.Server.TaskQueues.Programs;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;

namespace Cangjie.TypeSharp.Server.TaskQueues;

/// <summary>
/// 任务服务
/// </summary>
public class TaskService
{
    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService()
    {
        TaskCompletion = new();
        AgentCollection = new(this);
        TaskCollection = new(this);
        PluginCollection = new(this);
        ShareServer = new(this);
        ProgramCollection = new();

        PluginCollection.OnLoadedPlugins = () =>
        {
            Json plugins = Json.NewArray();
            PluginCollection.GetPlugins(item => plugins.Add(item.Target.Clone()));
            _ = Task.Run(async () =>
            {
                try
                {
                    Logger.Info("Update Plugins");
                    await ShareServer.Update(plugins);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        };
    }

    /// <summary>
    /// 任务完成
    /// </summary>
    public TaskCompletionSourcePool<Guid, object?> TaskCompletion { get; }

    /// <summary>
    /// 代理人集合
    /// </summary>
    public AgentCollection AgentCollection { get; }

    /// <summary>
    /// 任何集合
    /// </summary>
    public TaskCollection TaskCollection { get; }

    /// <summary>
    /// 插件集合
    /// </summary>
    public PluginCollection PluginCollection { get; }

    /// <summary>
    /// 共享服务
    /// </summary>
    public ShareServer ShareServer { get; }

    /// <summary>
    /// 程序集合
    /// </summary>
    public ProgramCollection ProgramCollection { get; } = new();

    /// <summary>
    /// 当前服务器地址
    /// </summary>
    public string CurrentServerUrlPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 运行任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task Run(TaskInterface task)
    {
        await TaskCollection.Run(task);
    }

    /// <summary>
    /// 尝试获取插件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public bool TryGetPlugin(string name, out PluginInterface plugin)
    {
        if (PluginCollection.TryGetPlugin(name, out plugin))
        {
            return true;
        }
        if (AgentCollection.TryGetPlugin(name, out plugin))
        {
            return true;
        }
        plugin = default;
        return false;
    }


}

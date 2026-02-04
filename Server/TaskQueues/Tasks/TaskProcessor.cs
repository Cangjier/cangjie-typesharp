using TidyHPC.Loggers;
using TidyHPC.Queues;

namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 任务处理器
/// </summary>
public class TaskProcessor : IProcessor<TaskInterface>
{
    /// <summary>
    /// 任务处理器
    /// </summary>
    /// <param name="taskService"></param>
    public TaskProcessor(TaskService taskService)
    {
        TaskService = taskService;
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 处理任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task Process(TaskInterface task)
    {
        var pluginCollection = TaskService.PluginCollection;
        var agentCollection = TaskService.AgentCollection;
        var shareServer = TaskService.ShareServer;
        try
        {
            if (task.Processor.Type == ProcessorTypes.Plugin)
            {
                //首先尝试从本地插件中运行任务
                if (pluginCollection.TryGetPlugin(task.Processor.Name, out var plugin))
                {
                    Logger.Info($"Plugin {plugin.Name} is processng task {task.id}");
                    await plugin.Run(TaskService, task);
                    Logger.Info($"Plugin {plugin.Name} has completed task {task.id}");
                }
                //其次从代理集合中运行任务
                else if (agentCollection.TryGetAgent(task, out var agent))
                {
                    while (true)
                    {
                        try
                        {
                            Logger.Info($"Agent {agent.ID} is processing task {task.id}");
                            await agent.Run(task);
                            Logger.Info($"Agent {agent.ID} has completed task {task.id}");
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            if (agentCollection.TryGetAgent(task, out agent))
                            {
                                continue;
                            }
                            else
                            {
                                task.Trace.Error($"{task.Processor.Name} Plugin not found");
                                break;
                            }
                        }
                    }
                }
                //其次从共享服务中运行任务
                else if (shareServer.Enabled)
                {
                    Logger.Info($"Shareserver {shareServer.UrlPrefix} is processng task {task.id}");
                    await shareServer.Run(task);
                    Logger.Info($"Shareserver {shareServer.UrlPrefix} has completed task {task.id}");
                }
                //最后报错
                else
                {
                    task.Trace.Error($"{task.Processor.Name} Plugin not found");
                }
            }
            else
            {
                task.Trace.Error("Processor type not supported");
            }
        }
        catch (Exception e)
        {
            task.Trace.Error(null, e);
        }
        finally
        {
            try
            {
                TaskService.TaskCollection.CompleteTask(task);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

        }
    }
}
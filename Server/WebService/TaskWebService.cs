using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server.TaskQueues;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;

namespace Cangjie.TypeSharp.Server.WebService;

/// <summary>
/// 任务HTTP服务
/// </summary>
public class TaskWebService
{
    /// <summary>
    /// Task Http Service
    /// </summary>
    /// <param name="taskService"></param>
    public TaskWebService(TaskService taskService)
    {
        TaskService = taskService;
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 发起一个插件任务
    /// </summary>
    /// <returns></returns>
    public async Task<Guid> PluginRunAsync(Session session, string pluginName)
    {
        TaskInterface task = Json.NewObject();
        task.Input = (await session.Cache.GetRequstBodyJson()).Clone();
        task.Processor.Name = pluginName;
        task.Processor.Type = ProcessorTypes.Plugin;
        task.id = Guid.NewGuid();
        _ = Task.Run(async () =>
        {
            await TaskService.Run(task);
        });
        return task.id;
    }

    /// <summary>
    /// 运行任务
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task<NetMessageInterface> Run(Session session)
    {
        NetMessageInterface result = NetMessageInterface.New();
        try
        {
            TaskInterface task = await session.Cache.GetRequstBodyJson();
            result.data = task;
            await TaskService.Run(task);
            result.success = task.Trace.Success;
            result.message = task.Trace.Message;
            result.data = task;
            return result;
        }
        catch (Exception e)
        {
            result.Error(null, null, e);
            return result;
        }
    }

    /// <summary>
    /// 发起一个任务
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task<Guid> RunAsync(Session session)
    {
        TaskInterface task = await session.Cache.GetRequstBodyJson();
        _ = Task.Run(async () =>
        {
            await TaskService.Run(task);
        });
        return task.id;
    }

    /// <summary>
    /// 查询任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<NetMessageInterface> Query(Guid id)
    {
        await Task.CompletedTask;
        NetMessageInterface result = NetMessageInterface.New();
        if (TaskService.TaskCollection.TryGet(id, out var task))
        {
            result.success = task.Trace.Success;
            result.message = task.Trace.Message;
            result.data = task;
        }
        else
        {
            result.Error("Task not found");
        }
        return result;
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="task_id"></param>
    /// <returns></returns>
    public async Task UpdateProgress(Guid task_id, Json progress)
    {
        await TaskService.TaskCollection.UpdateProgress(task_id, progress);
    }

    /// <summary>
    /// 订阅进度
    /// </summary>
    /// <param name="task_id"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task<NoneResponse> SubscribeProgress(Guid task_id, Session session)
    {
        await Task.CompletedTask;
        if (session.Response is IWebsocketResponse websocketResponse)
        {
            try
            {
                TaskService.TaskCollection.SubscribeProgress(task_id, websocketResponse);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                websocketResponse.Close();
            }
        }
        return new();
    }
}

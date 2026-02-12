using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Schedulers;

namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 任务服务
/// </summary>
public class TaskCollection
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public TaskCollection(TaskService taskService)
    {
        TaskService = taskService;
        TaskQueue = new MultiplyTaskProcessorQueue<string, TaskInterface>(Environment.ProcessorCount * 64, new TaskProcessor(taskService), async (task) =>
        {
            if (task != null)
            {
                task.Status = TaskStatuses.Running;
            }
            await Task.CompletedTask;
        });
        _ = Task.Run(DiscreteScheduler.StartAsync);
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 基础队列
    /// </summary>
    private MultiplyTaskProcessorQueue<string, TaskInterface> TaskQueue { get; }



    /// <summary>
    /// 任务列表
    /// </summary>
    private ConcurrentDictionary<Guid, TaskInterface> CacheTasks { get; } = new();

    /// <summary>
    /// 进度订阅者
    /// </summary>
    private ConcurrentDictionary<Guid, ProgressSubscriber> ProgressSubscribers { get; } = new();

    /// <summary>
    /// 离散调度器
    /// </summary>
    private DiscreteScheduler DiscreteScheduler { get; } = new();

    private ConcurrentDictionary<string, bool> IsQueueSetConcurrent { get; } = new();

    private void SetQueueConcurrent(string queue, int concurrent)
    {
        if (IsQueueSetConcurrent.TryAdd(queue, true))
        {
            TaskQueue.SetConcurrent(queue, concurrent);
        }
    }

    /// <summary>
    /// 执行任务，并等待任务完成
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task Run(TaskInterface task)
    {
        if (task.id.Equals(Guid.Empty))
        {
            task.id = Guid.NewGuid();
        }
        Logger.Info($"Task {task.id} is running");
        CacheTasks.TryAdd(task.id, task);
        task.Status = TaskStatuses.Pending;
        var waitTask = TaskService.TaskCompletion.Add(task.id).Task;
        if (TaskService.TryGetPlugin(task.Processor.Name, out var plugin))
        {
            if (plugin.Queue != "main" && plugin.Concurrent != -1)
            {
                SetQueueConcurrent(plugin.Queue, plugin.Concurrent);
            }
            TaskQueue.Enqueue(plugin.Queue, task);
        }
        else
        {
            TaskQueue.Enqueue("main", task);
        }
        await waitTask;
    }

    /// <summary>
    /// 运行任务并订阅进度
    /// </summary>
    /// <param name="task"></param>
    /// <param name="websocketResponse"></param>
    /// <param name="isCloseAfterSubscribeCompleted"></param>
    /// <returns></returns>
    public async Task RunAndSubscribeProgress(TaskInterface task, IWebsocketResponse websocketResponse, bool isCloseAfterSubscribeCompleted)
    {
        if (task.id.Equals(Guid.Empty))
        {
            task.id = Guid.NewGuid();
        }
        Logger.Info($"Task {task.id} is running");
        ProgressSubscribers.TryAdd(task.id, new()
        {
            IsCloseAfterComplete = isCloseAfterSubscribeCompleted,
            WebsocketResponse = websocketResponse
        });
        CacheTasks.TryAdd(task.id, task);
        task.Status = TaskStatuses.Pending;
        var waitTask = TaskService.TaskCompletion.Add(task.id).Task;
        if (TaskService.TryGetPlugin(task.Processor.Name, out var plugin))
        {
            if (plugin.Queue != "main" && plugin.Concurrent != -1)
            {
                SetQueueConcurrent(plugin.Queue, plugin.Concurrent);
            }
            TaskQueue.Enqueue(plugin.Queue, task);
        }
        else
        {
            TaskQueue.Enqueue("main", task);
        }
        await waitTask;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="task"></param>
    public void CompleteTask(TaskInterface task)
    {
        Logger.Info($"Task {task.id} is completed");
        task.Status = task.Output.IsNull ? TaskStatuses.Failed : TaskStatuses.Completed;
        TaskService.TaskCompletion.Complete(task.id, null);
        // 任务完成后，超过10分钟的任务，将会从任务列表中移除，后续将无法被查询
        DiscreteScheduler.AddTask(TimeSpan.FromMinutes(10), async () =>
        {
            CacheTasks.TryRemove(task.id, out _);
            await Task.CompletedTask;
        });
        if (ProgressSubscribers.TryRemove(task.id, out var subscriber))
        {
            subscriber.Complete();
        }
    }

    /// <summary>
    /// 尝试获取任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public bool TryGet(Guid id, [MaybeNullWhen(false)] out TaskInterface task)
    {
        return CacheTasks.TryGetValue(id, out task);
    }

    /// <summary>
    /// 更新进度，既会更新任务的进度，也会通知订阅者
    /// </summary>
    /// <param name="id"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<bool> UpdateProgress(Guid id, Json progress)
    {
        try
        {
            if (CacheTasks.TryGetValue(id, out var task))
            {
                if (ProgressSubscribers.TryGetValue(id, out var subscriber))
                {
                    using Json progressMsg = Json.NewObject();
                    progressMsg.Set("url", Apis.V2.Tasks.UpdateProgress);
                    progressMsg.Set("task_id", id);
                    progressMsg.Set("progress", progress.Clone());
                    await subscriber.SendMessage(progressMsg.ToString());
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 订阅进度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="response"></param>
    public void SubscribeProgress(Guid id, IWebsocketResponse response)
    {
        Logger.Info($"Task {id} is subscribed progress");
        if (TaskService.TaskCompletion.Contains(id))
        {
            ProgressSubscribers.TryAdd(id, new()
            {
                WebsocketResponse = response,
                IsCloseAfterComplete = true
            });
        }
        else
        {
            throw new Exception("Task not found");
        }
    }
}
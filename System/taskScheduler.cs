using System.Collections.Concurrent;
using TidyHPC.LiteJson;
using TidyHPC.Locks;
namespace Cangjie.TypeSharp.System;

/// <summary>
/// 任务调度器
/// </summary>
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public class taskScheduler
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    private readonly LockKey _lockKey = new();

    private readonly ConcurrentDictionary<Guid, Task> _taskMap = new();

    public Guid run(Delegate onTask)
    {
        if (onTask.DynamicInvoke() is not Task task)
        {
            throw new InvalidOperationException("onTask must return a Task");
        }
        var id = Guid.NewGuid();
        _taskMap.TryAdd(id, task);
        return id;
    }

    public bool contains(Guid id)
    {
        return _taskMap.ContainsKey(id);
    }

    public bool remove(Guid id)
    {
        return _taskMap.TryRemove(id, out _);
    }

    public bool isCompleted(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) && task.IsCompleted;
    }

    public bool isSuccess(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) && task.IsCompletedSuccessfully;
    }

    public bool isFaulted(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) && task.IsFaulted;
    }

    public bool isCanceled(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) && task.IsCanceled;
    }

    public bool isRunning(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) && !task.IsCompleted;
    }

    public Exception? getException(Guid id)
    {
        return _taskMap.TryGetValue(id, out var task) ? task.Exception?.InnerException : null;
    }

    public Json getResultOrThrowException(Guid id)
    {
        if (!_taskMap.TryGetValue(id, out var task))
        {
            throw new InvalidOperationException($"Task with id {id} not found");
        }
        if (task.IsCompleted)
        {
            return new(task.GetType().GetProperty("Result")?.GetValue(task));
        }
        else if (task.IsFaulted)
        {
            throw new Exception(task.Exception?.InnerExceptions.FirstOrDefault()?.Message, task.Exception?.InnerExceptions.FirstOrDefault()?.InnerException);
        }
        else
        {
            throw new InvalidOperationException($"Task with id {id} is not completed or faulted");
        }
    }

    public async Task<Json> waitAsync(Guid id)
    {
        if (!_taskMap.TryGetValue(id, out var task))
        {
            throw new InvalidOperationException($"Task with id {id} not found");
        }
        try
        {
            await task;
        }
        catch (AggregateException aggregateException)
        {
            throw new Exception(aggregateException.InnerExceptions.FirstOrDefault()?.Message, aggregateException.InnerExceptions.FirstOrDefault()?.InnerException);
        }
        return getResultOrThrowException(id);
    }
}
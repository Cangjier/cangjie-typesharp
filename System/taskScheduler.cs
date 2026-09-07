using System.Collections.Concurrent;
using System.Reflection;
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
        var id = Guid.NewGuid();
        return run(id, onTask);
    }

    public Guid run(Guid id, Delegate onTask)
    {
        if (onTask.Method.GetParameters().Length == 0)
        {
            if (onTask.DynamicInvoke() is not Task task)
            {
                throw new InvalidOperationException("onTask must return a Task");
            }

            _taskMap.TryAdd(id, task);
            return id;
        }
        else if (onTask.Method.GetParameters().Length == 1)
        {
            if (onTask.DynamicInvoke(id) is not Task task)
            {
                throw new InvalidOperationException("onTask must return a Task");
            }

            _taskMap.TryAdd(id, task);
            return id;
        }
        else
        {
            throw new InvalidOperationException("onTask must have 0 or 1 parameters");
        }
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
            var exception = ExceptionUtil.GetInnerException(task.Exception);
            throw new Exception(exception?.Message, exception?.InnerException);
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
            var exception = ExceptionUtil.GetInnerException(aggregateException);
            throw new Exception(exception?.Message, exception?.InnerException);
        }
        catch (TargetInvocationException targetInvocationException)
        {
            var exception = ExceptionUtil.GetInnerException(targetInvocationException);
            throw new Exception(exception?.Message, exception?.InnerException);
        }

        return getResultOrThrowException(id);
    }

    public Guid[] list()
    {
        return _taskMap.Keys.ToArray();
    }

    public Json listInfos()
    {
        Json result = Json.NewArray();
        foreach (var task in _taskMap)
        {
            result.AddObject().Set("id", task.Key)
                .Set("isCompleted", task.Value.IsCompleted)
                .Set("isSuccess", task.Value.IsCompletedSuccessfully)
                .Set("isFaulted", task.Value.IsFaulted)
                .Set("isCanceled", task.Value.IsCanceled)
                .Set("exception", task.Value.Exception?.InnerException?.Message);
        }

        return result;
    }
}
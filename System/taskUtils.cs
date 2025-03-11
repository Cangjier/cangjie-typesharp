using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 任务工具
/// </summary>
public class taskUtils
{
    public static async Task whenAll(Json tasks)
    {
        List<Task> taskList = [];
        foreach(var task in tasks.GetArrayEnumerable())
        {
            taskList.Add(task.As<Task>());
        }
        await Task.WhenAll(taskList);
    }

    public static async Task whenAny(Json tasks)
    {
        List<Task> taskList = [];
        foreach (var task in tasks.GetArrayEnumerable())
        {
            taskList.Add(task.As<Task>());
        }
        await Task.WhenAny(taskList);
    }
}

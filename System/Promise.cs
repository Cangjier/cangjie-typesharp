using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class Promise
{
    public static async Task<Json> all(Json tasks)
    {
        List<Task> taskList = [];
        foreach (var task in tasks.GetArrayEnumerable())
        {
            taskList.Add(task.As<Task>());
        }
        await Task.WhenAll(taskList);
        Json result = Json.NewArray();
        foreach (var task in taskList)
        {
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty != null)
            {
                result.Add(resultProperty.GetValue(task));
            }
            else
            {
                result.Add(Json.Null);
            }
        }
        return result;
    }
}
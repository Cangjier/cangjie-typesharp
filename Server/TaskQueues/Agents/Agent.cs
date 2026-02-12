using System.Collections.Concurrent;
using System.Threading.Tasks;
using TidyHPC.Common;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls.Interfaces;
using Cangjie.TypeSharp.Server.TaskQueues.Plugins;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;

namespace Cangjie.TypeSharp.Server.TaskQueues.Agents;

/// <summary>
/// 代理人
/// </summary>
public class Agent
{
    /// <summary>
    /// 代理人
    /// </summary>
    /// <param name="taskService"></param>
    /// <param name="agentCollection"></param>
    public Agent(TaskService taskService, AgentCollection agentCollection)
    {
        TaskService = taskService;
        AgentCollection = agentCollection;
    }

    /// <summary>
    /// Implicitly convert Agent to Json
    /// </summary>
    /// <param name="agent"></param>
    public static implicit operator Json(Agent agent)
    {
        Json result = Json.NewObject();
        result.Set("ID", agent.ID);
        result.Set("HostName", agent.HostName);
        result.Set("Performance", agent.Performance);
        var pluginsArray = result.GetOrCreateArray("Plugins");
        foreach (var plugin in agent.Plugins)
        {
            pluginsArray.Add(plugin.Value.Target.Clone());
        }
        return result;
    }

    /// <summary>
    /// 临时ID
    /// </summary>
    public Guid ID { get; set; } = Guid.Empty;

    /// <summary>
    /// 主机名称
    /// </summary>
    public string HostName { get; set; } = string.Empty;

    /// <summary>
    /// 性能数据
    /// </summary>
    public PerformanceInterface Performance { get; set; } = Json.Null;

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 代理人集合
    /// </summary>
    public AgentCollection AgentCollection { get; }

    /// <summary>
    /// 代理人的响应
    /// </summary>
    public IWebsocketResponse? WebsocketResponse { get; set; }

    /// <summary>
    /// 代理人所有的可用插件
    /// </summary>
    public ConcurrentDictionary<string, PluginInterface> Plugins { get; } = new();

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    public void AddPlugin(PluginInterface plugin)
    {
        Plugins.TryAdd(plugin.Name, plugin);
        if (AgentCollection.MapPluginToAgents.TryGetValue(plugin.Name, out var agents))
        {
            agents.Add(ID);
        }
        else
        {
            AgentCollection.MapPluginToAgents[plugin.Name] = [ID];
        }
    }

    /// <summary>
    /// 移除插件
    /// </summary>
    /// <param name="pluginName"></param>
    public void RemovePlugin(string pluginName)
    {
        Plugins.TryRemove(pluginName, out _);
        if (AgentCollection.MapPluginToAgents.TryGetValue(pluginName, out var agents))
        {
            agents.Remove(ID);
        }
    }

    /// <summary>
    /// 清除插件
    /// </summary>
    public void Clear()
    {
        foreach (var plugin in Plugins)
        {
            if(AgentCollection.MapPluginToAgents.TryGetValue(plugin.Key,out var agents))
            {
                agents.Remove(ID);
            }
        }
        Plugins.Clear();
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task Run(TaskInterface task)
    {
        if (WebsocketResponse == null)
        {
            throw new Exception("WebsocketResponse is null");
        }
        var websocket_session_id = Guid.NewGuid();
        task.Target.Set("url", Apis.V2.Agents.Client.Run);
        task.Target.Set("response", Apis.V2.Response);
        task.Target.Set("websocket_session_id", websocket_session_id);
        var completionSource = TaskService.TaskCompletion.Add(websocket_session_id);
        try
        {
            await WebsocketResponse.SendMessage(task.ToString());
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Exit();
            completionSource.TrySetCanceled();
            throw;
        }
        try
        {
            var taskResult = await completionSource.Task.WaitAsync(TimeSpan.FromHours(24));
            if (taskResult is NetMessageInterface msg)
            {
                TaskInterface outputTask = msg.data;
                task.Trace.Update(outputTask.Trace);
                task.Output = outputTask.Output.Clone();
                msg.Dispose();
            }
            else
            {
                throw new Exception("Agent result is not NetMessageInterface");
            }
        }
        catch(Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }

    /// <summary>
    /// 是否包含插件
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public bool ContainsPlugin(string pluginName)
    {
        return Plugins.ContainsKey(pluginName);
    }

    /// <summary>
    /// 尝试获取插件
    /// </summary>
    /// <param name="pluginName"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public bool TryGetPlugin(string pluginName, out PluginInterface plugin)
    {
        return Plugins.TryGetValue(pluginName, out plugin);
    }

    /// <summary>
    /// 安装包
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task InstallPackage(string packageName)
    {
        if (WebsocketResponse == null)
        {
            throw new Exception("WebsocketResponse is null");
        }
        var websocket_session_id = Guid.NewGuid();
        Json request = Json.NewObject();
        request.Set("url", Apis.V2.Agents.Client.InstallPackage);
        request.Set("response", Apis.V2.Response);
        request.Set("websocket_session_id", websocket_session_id);
        request.Set("packageName", packageName);
        var completionSource = TaskService.TaskCompletion.Add(websocket_session_id);
        try
        {
            await WebsocketResponse.SendMessage(request.ToString());
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Exit();
            completionSource.TrySetCanceled();
            throw;
        }
        try
        {
            var taskResult = await completionSource.Task.WaitAsync(TimeSpan.FromHours(24));
            if (taskResult is NetMessageInterface msg)
            {
                if (msg.data.IsTrue)
                {
                    
                }
                else
                {
                    throw new Exception($"Install package failed, {msg.message}");
                }
                msg.Dispose();
            }
            else
            {
                throw new Exception("Agent result is not NetMessageInterface");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }

    /// <summary>
    /// 注销
    /// </summary>
    private void Exit()
    {
        TaskService.AgentCollection.Exit(ID);
    }
}

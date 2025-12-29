using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using TidyHPC.Routers.Urls.Interfaces;
using Cangjie.TypeSharp.Server.TaskQueues.Plugins;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;

namespace Cangjie.TypeSharp.Server.TaskQueues.Agents;

/// <summary>
/// 代理人集合
/// </summary>
public class AgentCollection
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="taskService"></param>
    public AgentCollection(TaskService taskService)
    {
        TaskService = taskService;
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 所有代理人
    /// </summary>
    private ConcurrentDictionary<Guid, Agent?> Agents { get; } = new();

    internal ConcurrentDictionary<string, HashSet<Guid>> MapPluginToAgents { get; } = new();

    /// <summary>
    /// 注册代理人
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="websocketResponse"></param>
    /// <param name="plugins"></param>
    public void Register(Guid agentId, IWebsocketResponse websocketResponse, Json plugins)
    {
        if (Agents.TryGetValue(agentId, out var agent) == false)
        {
            Agents[agentId] = null;
        }
        if (agent != null)
        {
            agent.ID = agentId;
            if (agent.WebsocketResponse != null && !agent.WebsocketResponse.Equals(websocketResponse))
            {
                agent.WebsocketResponse.Close();
            }
            agent.WebsocketResponse = websocketResponse;
            agent.Clear();
        }
        else
        {
            agent = new(TaskService, this);
            agent.ID = agentId;
            agent.WebsocketResponse = websocketResponse;
            Agents[agentId] = agent;
        }
        foreach (var plugin in plugins)
        {
            agent.AddPlugin(plugin.Clone());
        }
    }

    /// <summary>
    /// 是否包含代理人
    /// </summary>
    /// <param name="agentId"></param>
    /// <returns></returns>
    public bool ContainsAgent(Guid agentId)
    {
        return Agents.ContainsKey(agentId);
    }

    /// <summary>
    /// 是否包含代理人，可以运行任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public bool ContainsAgent(TaskInterface task)
    {
        foreach (var agent in Agents.Values)
        {
            if (agent == null)
            {
                continue;
            }
            if (agent.ContainsPlugin(task.Processor.Name))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 更新代理人性能信息
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="performance"></param>
    /// <param name="hostName"></param>
    public void UpdatePerformance(Guid agentId, string hostName, PerformanceInterface performance)
    {
        if (Agents.TryGetValue(agentId, out var agent))
        {
            if (agent != null)
            {
                agent.HostName = hostName;
                agent.Performance = performance;
            }
        }
    }

    /// <summary>
    /// 更新插件信息
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="plugins"></param>
    public void UpdatePlugins(Guid agentId, Json plugins)
    {
        if (Agents.TryGetValue(agentId, out var agent))
        {
            if (agent != null)
            {
                agent.Clear();
                foreach (var plugin in plugins)
                {
                    agent.AddPlugin(plugin.Clone());
                }
            }
        }
    }

    /// <summary>
    /// 尝试获取代理人
    /// </summary>
    /// <param name="name"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public bool TryGetPlugin(string name, out PluginInterface plugin)
    {
        foreach (var agentItem in Agents.Values)
        {
            if (agentItem == null)
            {
                continue;
            }
            if (agentItem.TryGetPlugin(name, out plugin))
            {
                return true;
            }
        }
        plugin = default;
        return false;
    }

    /// <summary>
    /// 尝试获取代理人
    /// </summary>
    /// <param name="task"></param>
    /// <param name="agent"></param>
    /// <returns></returns>
    public bool TryGetAgent(TaskInterface task, [MaybeNullWhen(false)] out Agent agent)
    {
        foreach (var agentItem in Agents.Values)
        {
            if (agentItem == null)
            {
                continue;
            }
            if (agentItem.ContainsPlugin(task.Processor.Name))
            {
                agent = agentItem;
                return true;
            }
        }
        agent = null;
        return false;
    }

    /// <summary>
    /// 获取所有代理人
    /// </summary>
    /// <returns></returns>
    public Agent?[] GetAgents()
    {
        return Agents.Values.ToArray();
    }

    /// <summary>
    /// 安装包
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public async Task InstallPackage(Guid agentId, string packageName)
    {
        if (Agents.TryGetValue(agentId, out var agent))
        {
            if (agent != null)
            {
                await agent.InstallPackage(packageName);
            }
        }
    }

    /// <summary>
    /// 退出
    /// </summary>
    /// <param name="agentID"></param>
    /// <exception cref="Exception"></exception>
    public void Exit(Guid agentID)
    {
        if (Agents.TryRemove(agentID, out var agent))
        {
            if (agent != null)
            {
                try
                {
                    agent.WebsocketResponse?.Close();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                agent.WebsocketResponse = null;
                agent.Clear();
            }
        }
        else
        {
            throw new Exception("invalid agent id");
        }
    }
}

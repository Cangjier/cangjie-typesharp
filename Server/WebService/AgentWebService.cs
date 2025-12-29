using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;
using Cangjie.TypeSharp.Server.TaskQueues;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;

namespace Cangjie.TypeSharp.Server.WebService;

/// <summary>
/// Agent的HTTP服务
/// </summary>
public class AgentWebService
{
    /// <summary>
    /// 服务端的服务
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Agent Web Service
        /// </summary>
        /// <param name="taskService"></param>
        public Server(TaskService taskService)
        {
            TaskService = taskService;
        }

        /// <summary>
        /// 任务服务
        /// </summary>
        public TaskService TaskService { get; }

        /// <summary>
        /// 注册一个代理人
        /// </summary>
        /// <param name="session"></param>
        /// <param name="agentId"></param>
        /// <param name="plugins"></param>
        /// <returns></returns>
        public async Task<NetMessageInterface> Register(Session session, Guid agentId, Json plugins)
        {
            await Task.CompletedTask;
            if (agentId.Equals(Guid.Empty))
            {
                agentId = Guid.NewGuid();
            }
            if (session.Response is IWebsocketResponse websocketResponse)
            {
                TaskService.AgentCollection.Register(agentId, websocketResponse, plugins);
            }
            NetMessageInterface result = NetMessageInterface.New();
            result.data = agentId;
            return result;
        }

        /// <summary>
        /// 更新性能信息
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="performance"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public async Task<NetMessageInterface> UpdatePerformance(Guid agentId, string hostName, Json performance)
        {
            await Task.CompletedTask;
            TaskService.AgentCollection.UpdatePerformance(agentId, hostName, performance);
            NetMessageInterface result = NetMessageInterface.New();
            result.data = true;
            return result;
        }

        /// <summary>
        /// 更新插件信息
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="plugins"></param>
        /// <returns></returns>
        public async Task<NetMessageInterface> UpdatePlugins(Guid agentId, Json plugins)
        {
            await Task.CompletedTask;
            TaskService.AgentCollection.UpdatePlugins(agentId, plugins);
            NetMessageInterface result = NetMessageInterface.New();
            result.data = true;
            return result;
        }

        /// <summary>
        /// 获取所有代理人
        /// </summary>
        /// <returns></returns>
        public async Task<Json> Get()
        {
            await Task.CompletedTask;
            Json result = Json.NewArray();
            foreach (var agent in TaskService.AgentCollection.GetAgents())
            {
                if (agent != null)
                {
                    result.Add(agent);
                }
            }
            return result;
        }

        /// <summary>
        /// 安装包
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public async Task<bool> InstallPackage(Guid agentId, string packageName)
        {
            await TaskService.AgentCollection.InstallPackage(agentId, packageName);
            return true;
        }
    }

    /// <summary>
    /// 客户端的服务
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Task Http Service
        /// </summary>
        /// <param name="taskService"></param>
        public Client(TaskService taskService)
        {
            TaskService = taskService;
        }

        /// <summary>
        /// 任务服务
        /// </summary>
        public TaskService TaskService { get; }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <returns></returns>
        public async Task<NetMessageInterface> Run(Session session)
        {
            TaskInterface task = await session.Cache.GetRequstBodyJson();
            if (session.Response is IWebsocketResponse websocketResponse)
            {
                await TaskService.TaskCollection.RunAndSubscribeProgress(task, websocketResponse, false);
            }
            else
            {
                await TaskService.TaskCollection.Run(task);
            }
            NetMessageInterface result = NetMessageInterface.New();
            result.data = task.Target.Clone();
            return result;
        }
    }
}

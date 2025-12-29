using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server.TaskQueues;

namespace Cangjie.TypeSharp.Server.WebService;
internal class CommonWebService
{
    /// <summary>
    /// Agent Web Service
    /// </summary>
    /// <param name="taskService"></param>
    public CommonWebService(TaskService taskService)
    {
        TaskService = taskService;
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 完成某个响应
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task<NoneResponse> Response(Session session)
    {
        NetMessageInterface msg = await session.Cache.GetRequstBodyJson();
        TaskService.TaskCompletion.Complete(msg.Target.Read("websocket_session_id", Guid.Empty), msg);
        return new NoneResponse();
    }
}

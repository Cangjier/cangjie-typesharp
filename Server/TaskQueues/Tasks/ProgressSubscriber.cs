using TidyHPC.Routers.Urls.Interfaces;

namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 进度订阅者
/// </summary>
public class ProgressSubscriber
{
    /// <summary>
    /// 是否需要在任务订阅结束后关闭
    /// </summary>
    public bool IsCloseAfterComplete { get; set; }

    /// <summary>
    /// Websocket响应
    /// </summary>
    public IWebsocketResponse? WebsocketResponse { get; set; }

    /// <summary>
    /// 完成订阅
    /// </summary>
    public void Complete()
    {
        if (IsCloseAfterComplete)
        {
            WebsocketResponse?.Close();
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessage(string message)
    {
        if (WebsocketResponse != null)
        {
            await WebsocketResponse.SendMessage(message);
        }
    }
}

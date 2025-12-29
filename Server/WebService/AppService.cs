using System;
using System.Collections.Concurrent;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace Cangjie.TypeSharp.Server.WebService;

public class AppService
{
    /// <summary>
    /// 广播映射
    /// </summary>
    public ConcurrentDictionary<string, IWebsocketResponse> BroadcastMap { get; } = [];

    /// <summary>
    /// 广播消息到其他Web Page，不包括发送消息的页面
    /// <para>仅支持websocket</para>
    /// </summary>
    /// <returns></returns>
    public async Task Broadcast(Session session)
    {
        if (session.IsWebSocket == false)
        {
            return;
        }
        var message = await session.Cache.GetRequstBodyJson();
        var action = message.Read("action", string.Empty);
        var appId = message.Read("app_id", string.Empty);
        if (action == "register")
        {
            Logger.Debug($"Register broadcast client: {appId}");
            if (string.IsNullOrEmpty(appId) == false)
            {
                BroadcastMap[appId] = session.WebSocketResponse ?? throw new NullReferenceException();
            }
            else
            {
                throw new ArgumentException("app_id is null or empty");
            }
        }
        else if (action == "broadcast")
        {
            var others = BroadcastMap.Where(x => x.Key != appId).ToArray();
            Logger.Debug($"Broadcast to {others.Length} clients");
            foreach (var item in others)
            {
                var websocketResponse = item.Value;
                var itemKey = item.Key;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await websocketResponse.SendMessage(message.Get("data").ToString());
                    }
                    catch (Exception e)
                    {
                        BroadcastMap.TryRemove(itemKey, out var _);
                        Logger.Error("Broadcast failed", e);
                    }
                });
            }
        }
        else
        {
            Logger.Error($"Broadcast Unknown action: {action}");
        }
    }

}
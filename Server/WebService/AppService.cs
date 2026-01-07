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
    public ConcurrentDictionary<string, ConcurrentDictionary<string, IWebsocketResponse>> BroadcastMap { get; } = [];

    /// <summary>
    /// App锁
    /// </summary>
    private ConcurrentDictionary<string, object> LockMap { get; } = new();

    private object LockLockMap = new();

    private object GetAppLock(string appId)
    {
        lock (LockLockMap)
        {
            if (LockMap.TryGetValue(appId, out var lockObject))
            {
                return lockObject;
            }
            lockObject = new object();
            LockMap[appId] = lockObject;
            return lockObject;
        }
    }


    /// <summary>
    /// 广播消息到其他Web Page，不包括发送消息的页面
    /// <para>仅支持websocket</para>
    /// </summary>
    /// <returns></returns>
    public async Task Broadcast(Session session)
    {
        if (session.IsWebSocket == false)
        {
            Logger.Debug("Broadcast request is not a websocket request");
            return;
        }
        var message = await session.Cache.GetRequstBodyJson();
        var action = message.Read("action", string.Empty);
        var appId = message.Read("app_id", string.Empty);
        var pageId = message.Read("page_id", string.Empty);
        Logger.Debug($"Broadcast request: app_id: {appId}, page_id: {pageId}, action: {action}");
        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(pageId))
        {
            throw new ArgumentException("app_id or page_id is null or empty");
        }
        if (action == "register")
        {
            var appLock = GetAppLock(appId);
            lock (appLock)
            {
                Logger.Debug($"Register broadcast client: {appId}, page: {pageId}");
                if (BroadcastMap.TryGetValue(appId, out var pageMap) == false)
                {
                    pageMap = new ConcurrentDictionary<string, IWebsocketResponse>();
                    BroadcastMap[appId] = pageMap;
                }
                pageMap[pageId] = session.WebSocketResponse ?? throw new NullReferenceException();
            }
        }
        else if (action == "broadcast")
        {
            var appLock = GetAppLock(appId);
            ConcurrentDictionary<string, IWebsocketResponse>? pageMap = null;
            lock (appLock)
            {
                if (BroadcastMap.TryGetValue(appId, out pageMap) == false)
                {
                    pageMap = new ConcurrentDictionary<string, IWebsocketResponse>();
                    BroadcastMap[appId] = pageMap;
                }
            }

            var others = pageMap.Where(x => x.Key != pageId).ToArray() ;
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
                        pageMap?.TryRemove(itemKey, out var _);
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
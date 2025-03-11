using System.Net.WebSockets;
using System.Text;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;
public class WebSocket:IDisposable
{
    public WebSocket(string url)
    {
        client = new ClientWebSocket();
        _ = start(url);
    }

    public const string OPEN = "OPEN";

    public const string CLOSE = "CLOSE";

    public const string MESSAGE = "MESSAGE";

    private ClientWebSocket client;

    private Queue<WebSocketEvent> openEvents = new();

    private Queue<WebSocketEvent> closeEvents = new();

    private Queue<WebSocketEvent> messageEvents = new();

    private SemaphoreSlim openEventsNotify = new(0);

    private SemaphoreSlim closeEventsNotify = new(0);

    private SemaphoreSlim messageEventsNotify = new(0);

    private List<Action<WebSocketEvent>> openCallbacks = new();

    private List<Action<WebSocketEvent>> closeCallbacks = new();

    private List<Action<WebSocketEvent>> messageCallbacks = new();

    private bool disposed = false;

    private async Task start(string url)
    {
        try
        {
            await client.ConnectAsync(new Uri(url), CancellationToken.None);
            openEvents.Enqueue(new WebSocketEvent("OPEN", Json.Null));

            const int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];

            while (client.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                string message = Encoding.UTF8.GetString(ms.ToArray());
                messageEvents.Enqueue(new WebSocketEvent("MESSAGE", message));
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            closeEvents.Enqueue(new WebSocketEvent("CLOSE", Json.Null));
        }
    }

    public void Dispose()
    {
        client?.Dispose();
        client = null!;
        openEvents?.Clear();
        openEvents = null!;
        closeEvents?.Clear();
        closeEvents = null!;
        messageEvents?.Clear();
        messageEvents = null!;
        openEventsNotify?.Dispose();
        openEventsNotify = null!;
        closeEventsNotify?.Dispose();
        closeEventsNotify = null!;
        messageEventsNotify?.Dispose();
        messageEventsNotify = null!;
        openCallbacks?.Clear();
        openCallbacks = null!;
        closeCallbacks?.Clear();
        closeCallbacks = null!;
        messageCallbacks?.Clear();
        messageCallbacks = null!;
    }

    public void onopen(Action<WebSocketEvent> e)
    {
        if(openCallbacks.Count == 0)
        {
            openCallbacks.Add(e);
            _ = Task.Run(async () =>
            {
                while (disposed==false)
                {
                    try
                    {
                        await openEventsNotify.WaitAsync();
                        while (openEvents.TryDequeue(out var item))
                        {
                            foreach (var callback in openCallbacks)
                            {
                                try
                                {
                                    callback(item);
                                }
                                catch(Exception e)
                                {
                                    Logger.Error(e);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }
        else
        {
            openCallbacks.Add(e);
        }
    }

    public void onclose(Action<WebSocketEvent> e)
    {
        if (closeCallbacks.Count == 0)
        {
            closeCallbacks.Add(e);
            _ = Task.Run(async () =>
            {
                while (disposed == false)
                {
                    try
                    {
                        await closeEventsNotify.WaitAsync();
                        while (closeEvents.TryDequeue(out var item))
                        {
                            foreach (var callback in closeCallbacks)
                            {
                                try
                                {
                                    callback(item);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(e);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }
        else
        {
            closeCallbacks.Add(e);
        }
    }

    public void onmessage(Action<WebSocketEvent> e)
    {
        if (messageCallbacks.Count == 0)
        {
            messageCallbacks.Add(e);
            _ = Task.Run(async () =>
            {
                while (disposed == false)
                {
                    try
                    {
                        await messageEventsNotify.WaitAsync();
                        while (messageEvents.TryDequeue(out var item))
                        {
                            foreach (var callback in messageCallbacks)
                            {
                                try
                                {
                                    callback(item);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(e);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }
        else
        {
            messageCallbacks.Add(e);
        }
    }

}

public class WebSocketEvent(string type,Json data)
{
    public override string ToString()
    {
        Json self = Json.NewObject();
        self["type"] = type;
        self["data"] = data;
        return self.ToString();
    }
}

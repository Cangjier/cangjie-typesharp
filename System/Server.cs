using System.Net.Http.Headers;
using TidyHPC.LiteHttpServer;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Responses;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 内置服务器
/// </summary>
public class Server
{
    public Server()
    {
        Router.Events.OnNoRoute = async (url, session) =>
        {
            await Task.CompletedTask;
            session.Complete(() =>
            {
                session.Response.StatusCode = 404;
                session.Response.Headers.SetHeader("Content-Type", "text/html");
                session.Response.Body.Write(Util.UTF8.GetBytes($"""
        <html>
        <head>
        <title>404 Not Found</title>
        </head>
        <body>
        <h1>404 Not Found</h1>
        <p>The requested URL was not found on this server.</p>
        <p>URL: {url}</p>
        </body>
        </html>
        """));
            });
        };
        Router.Register([@"^(?!/api/v\d*/)(?<filePath>.*)$"], async (Session session, string filePath) =>
        {
            await Task.CompletedTask;
            session.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(30)
            };
            if (filePath.Contains('.'))
            {
                string fullPath = Path.Combine(StaticResourcePath, filePath.TrimStart('/'));
                return new DetectFile(fullPath, filePath);
            }
            else
            {
                string fullPath = Path.Combine(StaticResourcePath, filePath.Trim('/'));
                while (true)
                {
                    var indexFilePath = Path.Combine(fullPath, "index.html");
                    if (File.Exists(indexFilePath))
                    {
                        return new DetectFile(indexFilePath, filePath);
                    }
                    if (fullPath == StaticResourcePath)
                    {
                        break;
                    }
                    fullPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
                }
                return new DetectFile(Path.Combine(StaticResourcePath, "index.html"), filePath);
            }
        });
        //对所有请求添加跨源，对Options请求直接过滤
        Router.Filter.Register(0, [".*"], async (Session session) =>
        {
            await Task.CompletedTask;
            session.Response.Headers.SetHeader("Access-Control-Allow-Origin", "*");
            session.Response.Headers.SetHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            session.Response.Headers.SetHeader("Access-Control-Allow-Headers", "*");
            session.Response.Headers.SetHeader("Access-Control-Allow-Credentials", "true");

            if (session.Request.Method == UrlMethods.HTTP_OPTIONS)
            {
                return false;
            }
            return true;
        });
    }

    private UrlRouter Router = new();

    private string StaticResourcePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build");

    public void useStatic(string directory)
    {
        StaticResourcePath = directory;
    }

    public async Task start(int port)
    {
        HttpServer server = new();
        server.Prefixes.Add($"http://*:{port}/");
        _ = Task.Run(server.Start);
        await Router.Listen(server);
    }

    /// <summary>
    /// 使用路由
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="delegate"></param>
    public void use(string pattern,Delegate @delegate)
    {
        Router.Register([pattern], @delegate);
    }
}

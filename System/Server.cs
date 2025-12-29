using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls.Responses;
using Cangjie.TypeSharp.Server;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 内置服务器
/// </summary>
public class Server
{
    public Server()
    {
        Application = new Application();
        Application.TaskService.ProgramCollection.CreateProgramByScriptContent = (filePath, content) =>
        {
            return new TSProgram(filePath, content);
        };
        Application.TaskService.ProgramCollection.RunProgramByFilePathAndArgs = async (program, filePath, args) =>
        {
            if (program is not TSProgram programInstance)
            {
                throw new ArgumentException();
            }
            using var context = new Context();
            context.script_path = filePath;
            context.args = args;
            await programInstance.RunAsync(context);
            await context.Logger.QueueLogger.WaitForEmpty();
        };
        Application.TaskService.ProgramCollection.RunProgramByFilePathAndContext = async (program, filePath, context) =>
        {
            if (program is not TSProgram programInstance)
            {
                throw new ArgumentException($"program is not a TSProgram");
            }
            var asContext = context as Context ?? throw new ArgumentException($"context is not a Context");
            asContext.script_path = filePath;
            asContext.args = [];
            return await programInstance.RunWithoutDisposeAsync(asContext);
        };
    }
    public Server(Application application)
    {
        Application = application;
    }
    public Application Application { get; }

    public ApplicationConfig ApplicationConfig { get; } = new();

    public TaskCompletionSource onConfigCompleted => Application.OnConfigCompleted;

    public async Task start(int port)
    {
        //UrlResponse.DefaultContentEncoding = "";
        ApplicationConfig.ServerPorts = [port];
        await Application.Start(ApplicationConfig);
    }

    /// <summary>
    /// 使用路由
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="delegate"></param>
    public void use(string pattern, Delegate @delegate)
    {
        Application.Register(pattern, @delegate);
    }

    public void useStatic(string directory)
    {
        ApplicationConfig.StaticResourcePath = directory;
    }
}
using Cangjie.Core.Exceptions;
using Cangjie.TypeSharp.Cli;
using Cangjie.TypeSharp.System;
using System.Drawing;
using TidyHPC.Extensions;
using TidyHPC.Loggers;
using TidyHPC.Routers;
using TidyHPC.Routers.Args;

namespace Cangjie.TypeSharp.System;

public class CommonCommands
{
    public static async Task Run(
    [ArgsIndex] string path,
    [ArgsAliases("--repository")] string? repository = null,
    [ArgsAliases("--application-name")] string? applicationName = null,
    [ArgsAliases("--use-update")] string? useUpdate = null,
    [Args] string[]? fullArgs = null)
    {
        using Context context = new(Logger.LoggerFile);
        try
        {
            // Logger.SetLoggerFile(context.Logger);
            context.args = fullArgs![2..];
            if (File.Exists(path))
            {
                context.script_path = Path.GetFullPath(path);
                await TSScriptEngine.RunAsync(context.script_path, File.ReadAllText(context.script_path, Util.UTF8), context);
            }
            else if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.ts", SearchOption.AllDirectories);
                // 找到main.ts或者index.ts文件
                var mainFile = files.FirstOrDefault(item => Path.GetFileName(item).ToLower() == "main.ts" || Path.GetFileName(item).ToLower() == "index.ts");
                if (mainFile == null)
                {
                    Console.WriteLine("main.ts or index.ts not found");
                    return;
                }
                context.script_path = Path.GetFullPath(mainFile);
                await TSScriptEngine.RunAsync(context.script_path, File.ReadAllText(context.script_path, Util.UTF8), context);
            }
            else if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                context.script_path = path;
                var url = Util.GetRawUrl(path);
                var content = await Util.HttpGetAsString(url);
                Console.WriteLine($"get script from {url}");
                await TSScriptEngine.RunAsync(path, content, context);
            }
            else
            {
                GitRepository gitRepository = new();
                if (repository != null)
                {
                    gitRepository.RepositoryUrl = repository;
                }
                if (applicationName != null)
                {
                    gitRepository.ApplicationName = applicationName;
                }
                if (useUpdate == null || useUpdate == "true")
                {
                    await gitRepository.Update();
                }
                var listResult = gitRepository.ListCli();
                if (listResult.Where(item => string.Compare(path, item, true) == 0).Count() == 0)
                {
                    Console.WriteLine($"script list : \r\n{listResult.JoinArray("\r\n", (index, item) => $"{index,-3}{item}")}");
                    Console.WriteLine($"script {path} not found");
                    return;
                }
                var scriptPath = gitRepository.GetCliScriptPath(path);
                if (scriptPath == null)
                {
                    Console.WriteLine($"script {path} not found");
                    return;
                }
                context.script_path = scriptPath;
                await TSScriptEngine.RunAsync(context.script_path, File.ReadAllText(context.script_path, Util.UTF8), context);
            }
        }
        catch (Exception e)
        {
            var message = TSProgram.GetExceptionMessage(e);
            Logger.WriteLine(message);
            if (Logger.LoggerFile.QueueLogger.OnLine == null)
            {
                Console.WriteLine(message);
            }
            if (e is not RuntimeException<char>)
            {
                Logger.Error(e);
            }
            Environment.Exit(-1);
        }
    }

    public static async Task ListScripts(
        [ArgsAliases("--repository")] string? repository = null,
        [ArgsAliases("--application-name")] string? applicationName = null)
    {
        GitRepository gitRepository = new();
        if (repository != null)
        {
            gitRepository.RepositoryUrl = repository;
        }
        if (applicationName != null)
        {
            gitRepository.ApplicationName = applicationName;
        }
        await gitRepository.Update();
        var listResult = gitRepository.ListCli().Where(item => item != ".tsc");
        int index = 0;
        int count = listResult.Count();
        foreach (var item in listResult)
        {
            Console.WriteLine($"{index++,3}/{count,-3}{item}");
        }
    }


    /// <summary>
    /// 获取当前执行文件的路径
    /// </summary>
    /// <returns></returns>
    public static async Task Where()
    {
        await Task.CompletedTask;
        Logger.Info($"process path: {Environment.ProcessPath}");
        Logger.Info($"current directory: {Environment.CurrentDirectory}");
        Logger.Info($"application data: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");
        Logger.Info($"local application data: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");
        Logger.Info($"program files: {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}");
        Logger.Info($"program files x86: {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}");
    }
}

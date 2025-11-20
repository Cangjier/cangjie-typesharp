using TidyHPC.Common;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls.Responses;
using VizGroup.V1;
using VizGroup.V1.IOStorage;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 内置服务器
/// </summary>
public class Server
{
    public Server()
    {
        Application = new Application();
        Application.ServiceScope.TaskService.ProgramCollection.CreateProgramByScriptContent = (filePath, content) =>
        {
            return new TSProgram(filePath, content);
        };
        Application.ServiceScope.TaskService.ProgramCollection.RunProgramByFilePathAndArgs = async (program, filePath, args) =>
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
        Application.ServiceScope.TaskService.ProgramCollection.RunProgramByFilePathAndContext = async (program, filePath, context) =>
        {
            if (program is not TSProgram programInstance)
            {
                throw new ArgumentException($"program is not a TSProgram");
            }
            var asContext = context as Context ?? throw new ArgumentException($"context is not a Context");
            asContext.script_path = filePath;
            asContext.args = [];
            await programInstance.RunAsync(asContext);
        };
    }
    public Server(Application application)
    {
        Application = application;
    }
    public Application Application { get; }

    public ApplicationConfig ApplicationConfig { get; } = new();

    public database getDatabase()
    {
        return new(Application.Database.Value);
    }

    public ServiceScope serviceScope => Application.ServiceScope;

    public ioStorageService storageService => new(serviceScope.IOStorageService);

    public TaskCompletionSource onDatabaseSetupCompleted => Application.OnDatabaseStepupCompleted;

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

public class ioStorageService(IOStorageService target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public IOStorageService Target { get; } = target;

    public async Task<FileInterface> importFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileContentMD5 = Util.ComputeMD5HashByFilePath(filePath);
        var fileMD5 = Util.ComputeMD5Hash(fileContentMD5 + fileName);
        if (await Target.ContainsContent(fileContentMD5) == false)
        {
            using var fileStream = File.OpenRead(filePath);
            await Target.ImportContent(fileStream);
        }
        FileInterface? result = null;
        if (await Target.ContainsFile(fileMD5))
        {
            result = await Target.GetFile(fileMD5);
        }
        else
        {
            result = await Target.ImportFile(fileName, fileMD5, fileContentMD5, [], DateTime.MinValue);
        }
        return result;
    }

    public async Task<ContentInterface> importString(string value)
    {
        var contentMD5 = Util.ComputeMD5Hash(value);
        Ref<ContentInterface> result = new(new());
        if (await Target.TryGetContent(contentMD5, result) == false)
        {
            using var stream = new MemoryStream(Util.UTF8.GetBytes(value));
            result.Value = await Target.ImportContent(stream);
        }
        return result.Value;
    }

    public async Task<bool> containsContentByMD5(string contentMD5)
    {
        return await Target.ContainsContent(contentMD5);
    }

    public async Task exportContentToFilePath(string contentMD5, string filePath)
    {
        using var fileStream = File.OpenWrite(filePath);
        await Target.ExportContent(contentMD5, fileStream);
    }

    public string getContentArchivePath(string contentMD5)
    {
        return Target.GetContentArchivePath(contentMD5);
    }

    public async Task exportFileToFilePath(string fileMD5, string filePath)
    {
        var fileInterface = await Target.GetFile(fileMD5);
        using var fileStream = File.OpenWrite(filePath);
        await Target.ExportContent(fileInterface.FullContentMD5, fileStream);
    }

    public async Task<string> readContent(string contentMD5)
    {
        using var stream = new MemoryStream();
        await Target.ExportContent(contentMD5, stream);
        return Util.UTF8.GetString(stream.ToArray());
    }

    public async Task<FileInterface> getFileByID(Json fileID)
    {
        if (fileID.IsGuid)
        {
            return await Target.GetFile(fileID.AsGuid);
        }
        else if (fileID.IsString)
        {
            return await Target.GetFile(Guid.Parse(fileID.AsString));
        }
        throw new ArgumentException();
    }
}

using Cangjie.TypeSharp.Cli;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class Apis:IDisposable
{
    public Apis(Context context)
    {
        this.context = context;
    }

    public Context context { get; private set; }

    private static string? findDirectory(string? rootDirectory, string name)
    {
        while (true)
        {
            if (rootDirectory == null)
            {
                return null;
            }
            if (Directory.Exists(Path.Combine(rootDirectory, name)))
            {
                return Path.Combine(rootDirectory, name);
            }
            if (rootDirectory == Path.GetPathRoot(rootDirectory))
            {
                return null;
            }
            rootDirectory = Path.GetDirectoryName(rootDirectory);
        }
    }

    private string? _directory = null;

    public string directory
    {
        get
        {
            if (_directory == null)
            {
                _directory = findDirectory(Environment.CurrentDirectory, ".apis") ??
        findDirectory(Path.GetDirectoryName(Environment.ProcessPath), ".apis") ??
        findDirectory(Path.GetDirectoryName(context.script_path), ".apis") ?? ".apis";
            }
            return _directory;
        }
        set { _directory = value; }
    }

    private string logsDirectory => Path.Combine(Path.GetDirectoryName(directory) ?? Path.GetTempPath(), ".apis-log");

    public async Task<Json> runAsync2(string apiName,Json args)
    {
        if (Directory.Exists(logsDirectory) == false)
        {
            Directory.CreateDirectory(logsDirectory);
        }
        if (apiName.EndsWith(".json")) apiName = apiName[..apiName.IndexOf(".json")];
        string apiFileName = apiName + ".json";
        var apiPath = Path.Combine(directory, apiFileName);
        if (File.Exists(apiPath) == false) throw new Exception($"api {apiPath} not found");
        var outputPath = Path.Combine(logsDirectory, $"{apiName}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        var argsPath = Path.Combine(logsDirectory, $"{apiName}-args-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        await File.WriteAllTextAsync(argsPath, args.ToString(), Util.UTF8);
        await staticContext.execAsync(new()
        {
            filePath = Environment.ProcessPath ?? throw new Exception("Environment.ProcessPath is null"),
            arguments = new string[] { "api", "-i", apiPath, "-o", outputPath, "-a", argsPath }
        });
        File.Delete(argsPath);
        if (File.Exists(outputPath))
        {
            return (await Json.LoadAsync(outputPath)).Get("Response", Json.Null);
        }
        else throw new Exception($"api {apiPath} run failed");
    }

    public async Task<Json> runAsync(string apiName, Json args)
    {
        if (Directory.Exists(logsDirectory) == false)
        {
            Directory.CreateDirectory(logsDirectory);
        }
        if (apiName.EndsWith(".json")) apiName = apiName[..apiName.IndexOf(".json")];
        string apiFileName = apiName + ".json";
        var apiPath = Path.Combine(directory, apiFileName);
        if (File.Exists(apiPath) == false) throw new Exception($"api {apiPath} not found");
        var outputPath = Path.Combine(logsDirectory, $"{apiName}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        var argsPath = Path.Combine(logsDirectory, $"{apiName}-args-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        await File.WriteAllTextAsync(argsPath, args.ToString(), Util.UTF8);
        await ApiCommands.Run(apiPath, outputPath, argsPath);
        File.Delete(argsPath);
        if (File.Exists(outputPath))
        {
            return (await Json.LoadAsync(outputPath)).Get("Response", Json.Null);
        }
        else throw new Exception($"api {apiPath} run failed");
    }

    public async Task<Json> runAsyncWithAlias(string apiName, string alias, Json args)
    {
        if (Directory.Exists(logsDirectory) == false)
        {
            Directory.CreateDirectory(logsDirectory);
        }
        if (apiName.EndsWith(".json")) apiName = apiName[..apiName.IndexOf(".json")];
        string apiFileName = apiName + ".json";
        var apiPath = Path.Combine(directory, apiFileName);
        if (File.Exists(apiPath) == false) throw new Exception($"api {apiPath} not found");
        var outputPath = Path.Combine(logsDirectory, $"{alias}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        var argsPath = Path.Combine(logsDirectory, $"{alias}-args-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        await File.WriteAllTextAsync(argsPath, args.ToString(), Util.UTF8);
        await ApiCommands.Run(apiPath, outputPath, argsPath);
        File.Delete(argsPath);
        if (File.Exists(outputPath))
        {
            return (await Json.LoadAsync(outputPath)).Get("Response", Json.Null);
        }
        else throw new Exception($"api {apiPath} run failed");
    }

    public List<string?> list()
    {
        var files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
        return files.Select(Path.GetFileNameWithoutExtension).ToList();
    }

    public void Dispose()
    {
        context = null!;
        _directory = null;
    }
}

using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class apis
{
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

    private static string logsDirectory = Path.Combine(Path.GetTempPath(), ".apis-log");

    public static string directory { get; set; } = findDirectory(Environment.CurrentDirectory, ".apis") ??
        findDirectory(Path.GetDirectoryName(Environment.ProcessPath), ".apis") ??
        findDirectory(Path.GetDirectoryName(context.script_path), ".apis") ?? ".apis";

    public static async Task<Json> runAsync(string apiName,Json args)
    {
        if (apiName.EndsWith(".json")) apiName = apiName[..apiName.IndexOf(".json")];
        string apiFileName = apiName + ".json";
        var apiPath = Path.Combine(directory, apiFileName);
        if (File.Exists(apiPath) == false) throw new Exception($"api {apiPath} not found");
        var outputPath = Path.Combine(logsDirectory, $"{apiName}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        var argsPath = Path.Combine(logsDirectory, $"{apiName}-args-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        await File.WriteAllTextAsync(argsPath, args.ToString(), Util.UTF8);
        await context.execAsync(Environment.ProcessPath!, "api", "-i", apiPath, "-o", outputPath, "-a", argsPath);
        if (File.Exists(outputPath))
        {
            return (await Json.LoadAsync(outputPath)).Get("Response", Json.Null);
        }
        else throw new Exception($"api {apiPath} run failed");
    }

    public static List<string?> list()
    {
        var files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
        return files.Select(Path.GetFileNameWithoutExtension).ToList();
    }
}

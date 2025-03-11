using Cangjie.TypeSharp.Cli.Apis;
using Cangjie.TypeSharp.System;
using TidyHPC.LiteJson;
using TidyHPC.Routers;

namespace Cangjie.TypeSharp.Cli;

/// <summary>
/// Api Commands
/// </summary>
public class ApiCommands
{
    private static async Task RunApi(Json result, string? inputPath, Json inputJson, Json arguments, string? outputPath, NetMessageInterface msg)
    {
        using Context context = new();
        if (inputPath != null)
        {
            context.script_path = inputPath;
        }
        using var treatment = new Treatment(inputJson.GetOrCreateObject("Parameters"), context);
        //植入变量
        treatment.CoverParametersBy(arguments);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "");
        treatment.Parameters.Set("OutputDirectory", Path.GetDirectoryName(outputPath) ?? "");
        //开始进行Parameters初始化
        treatment.InitialParameters(inputPath);
        //开始对一般值进行处理
        treatment.Process(inputJson, ["Parameters"]);
        using Request? request = Request.Parse(inputJson, msg);
        if (request == null)
        {
            throw new Exception("请求为空");
        }
        else
        {
            inputJson.Set("UrlWithQueryParameters", request.UrlWithQueryParameters);
        }
        string? requestPath = null;
        if (outputPath != null)
        {
            requestPath = Path.Combine(Path.GetDirectoryName(outputPath) ?? "", $"{Path.GetFileNameWithoutExtension(outputPath)}-request.json");
            Json toSave = Json.NewObject();
            toSave.Set("Request", inputJson);
            toSave.Save(requestPath);
        }
        using Response? response = await Client.Send(request, msg);
        if (response == null)
        {
            throw new Exception("响应为空");
        }
        result.Set("Response", response.ToJson(treatment));
    }

    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="argumentsPath"></param>
    /// <returns></returns>
    public static async Task Run(
    [ArgsAliases("-i", "--input")] string? inputPath,
    [ArgsAliases("-o", "--output")] string? outputPath = null,
    [ArgsAliases("-a", "--arguments")] string? argumentsPath = null)
    {
        DateTime startTime = DateTime.Now;
        using NetMessageInterface msg = NetMessageInterface.New();
        using Json result = Json.NewObject();
        try
        {
            if (inputPath == null)
            {
                throw new Exception("输入路径为空");
            }
            if (outputPath == null)
            {
                outputPath = Path.Combine(Environment.CurrentDirectory, $"{Path.GetFileNameWithoutExtension(inputPath)}-output.json");
            }
            outputPath = Path.GetFullPath(outputPath);
            if (argumentsPath != null)
            {
                argumentsPath = Path.GetFullPath(argumentsPath);
            }
            var inputJson = Json.TryLoad(inputPath, () => throw new Exception($"输入Json非法，路径为：{inputPath}"));
            result.Set("Request", inputJson);
            var arguments = File.Exists(argumentsPath) ? Json.TryLoad(argumentsPath, Json.NewObject) : Json.NewObject();
            await RunApi(result, inputPath, inputJson, arguments, outputPath, msg);
        }
        catch (Exception e)
        {
            msg.Error("执行期间存在异常", e);
        }
        finally
        {
            result.Set("Trace", msg.Trace.Target);
            var trace = result.GetOrCreateObject("Trace");
            var endTime = DateTime.Now;
            trace.Set("StartTime", startTime);
            trace.Set("EndTime", endTime);
            trace.Set("CostTime", endTime - startTime);
            if (outputPath != null)
            {
                result.Save(outputPath);
            }
        }
    }
}
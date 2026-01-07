using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using Cangjie.TypeSharp.Server.TaskQueues.Tasks;
using Cangjie.Core.Extensions;

namespace Cangjie.TypeSharp.Server.TaskQueues.Plugins;

/// <summary>
/// 插件封装
/// </summary>
/// <param name="target"></param>
public readonly struct PluginInterface(Json target)
{
    /// <summary>
    /// 封装的目标对象
    /// </summary>
    public readonly Json Target = target;

    /// <summary>
    /// Implicit conversion from Json to PluginInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator PluginInterface(Json target) => new(target);

    /// <summary>
    /// Implicit conversion from PluginInterface to Json
    /// </summary>
    /// <param name="plugin"></param>
    public static implicit operator Json(PluginInterface plugin) => plugin.Target;

    /// <summary>
    /// 插件名称
    /// </summary>
    public readonly string Name => Target.Read("Name", string.Empty);

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnable => Target.Read("Enable", true);

    /// <summary>
    /// 插件显示名称
    /// </summary>
    public readonly string DisplayName => Target.Read("DisplayName", string.Empty);

    /// <summary>
    /// 插件描述
    /// </summary>
    public readonly string Description => Target.Read("Description", string.Empty);

    /// <summary>
    /// 插件版本
    /// </summary>
    public readonly string Version => Target.Read("Version", string.Empty);

    /// <summary>
    /// 插件作者
    /// </summary>
    public readonly string Author => Target.Read("Author", string.Empty);

    /// <summary>
    /// 插件类型
    /// </summary>
    public readonly string Type => Target.Read("Type", string.Empty);

    /// <summary>
    /// 插件入口
    /// </summary>
    public readonly string Entry => Target.Read("Entry", string.Empty);

    /// <summary>
    /// 插件超时时间
    /// </summary>
    public readonly int Timeout => Target.Read("Timeout", 1000 * 60 * 60 * 24);

    /// <summary>
    /// 插件回调地址
    /// </summary>
    public readonly string Callback => Target.Read("Callback", string.Empty);

    /// <summary>
    /// 优先级
    /// </summary>
    public readonly int Priority => Target.Read("Priority", 0);

    /// <summary>
    /// 队列名称
    /// </summary>
    public readonly string Queue
    {
        get
        {
            var queue = Target.Read("Queue", "main");
            if (queue == "name")
            {
                return Name;
            }
            return queue;
        }
    }

    /// <summary>
    /// 并发数量
    /// </summary>
    public readonly int Concurrent => Target.Read("Concurrent", -1);

    /// <summary>
    /// 运行任务
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskService"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Run(TaskService taskService, TaskInterface task)
    {
        var type = Type.ToLower();
        if (type == "commandline" || type == "")
        {
            await RunCommandLine(taskService, task);
        }
        else if (type == "file" || type == "fileread")
        {
            await RunFileRead(taskService, task);
        }
        else if (type == "filewrite")
        {
            await RunFileWrite(taskService, task);
        }
        else if (type == "httpget")
        {
            await RunHttpGet(taskService, task);
        }
        else if (type == "typesharp")
        {
            await RunTypeSharp(taskService, task, false);
        }
        else if (type == "typesharp-progress")
        {
            await RunTypeSharp(taskService, task, true);
        }
        else
        {
            task.Trace.Error($"Plugin type `{type}` not supported");
        }
    }

    /// <summary>
    /// 进程结果
    /// </summary>
    private class ProcessResult
    {
        /// <summary>
        /// 退出代码
        /// </summary>
        public int ExitCode { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否存在输出
        /// </summary>
        public bool IsExistsOutput { get; set; }
        /// <summary>
        /// 是否存在日志
        /// </summary>
        public bool IsExistsLogger { get; set; }
        /// <summary>
        /// 是否存在进度
        /// </summary>
        public bool IsExistsProgress { get; set; }
    }

    private async Task RunFileRead(TaskService taskService, TaskInterface task)
    {
        await Task.CompletedTask;
        var entry = Entry;
        var pluginPath = taskService.PluginCollection.GetPluginPath(Name);
        var pluginDirectory = Path.GetDirectoryName(pluginPath);
        entry = entry
            .Replace("{.}", pluginDirectory)
            .Replace("{..}", Path.GetDirectoryName(pluginDirectory))
            .Replace("{userprofile}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        var entryDirectory = Path.GetDirectoryName(entry);
        if (Directory.Exists(entryDirectory) == false && entryDirectory != null)
        {
            Directory.CreateDirectory(entryDirectory);
        }
        if (entry.EndsWith(".json"))
        {
            if (File.Exists(entry) == false && Target.ContainsKey("default"))
            {
                var defaultValue = Target["default"];
                if (defaultValue.IsString)
                {
                    File.WriteAllText(entry, defaultValue.AsString, Util.UTF8);
                }
                else
                {
                    File.WriteAllText(entry, defaultValue.ToString(), Util.UTF8);
                }
            }
            task.Output = Json.TryLoad(entry, Json.NewObject);
        }
        else
        {
            task.Output = File.ReadAllText(entry, Util.UTF8);
        }
    }

    private async Task RunFileWrite(TaskService taskService, TaskInterface task)
    {
        await Task.CompletedTask;
        var entry = Entry;
        var pluginPath = taskService.PluginCollection.GetPluginPath(Name);
        var pluginDirectory = Path.GetDirectoryName(pluginPath);
        entry = entry
            .Replace("{.}", pluginDirectory)
            .Replace("{..}", Path.GetDirectoryName(pluginDirectory))
            .Replace("{userprofile}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        task.Output = "";
        var entryDirectory = Path.GetDirectoryName(entry);
        if (Directory.Exists(entryDirectory) == false && entryDirectory != null)
        {
            Directory.CreateDirectory(entryDirectory);
        }
        if (task.Input.IsString)
        {
            File.WriteAllText(entry, task.Input.AsString, Util.UTF8);
        }
        else
        {
            task.Input.Save(entry);
        }
    }

    private async Task RunHttpGet(TaskService taskService, TaskInterface task)
    {
        var message = await Util.Axios.GetMessageAsync(Entry);
        if (message.success)
        {
            task.Output = message.data;
        }
        else
        {
            task.Trace.Error(message.message);
        }
    }

    private async Task RunCommandLine(TaskService taskService, TaskInterface task)
    {
        var entry = Entry;
        var timeout = Timeout;
        var taskID = task.id;
        var pluginPath = taskService.PluginCollection.GetPluginPath(Name);
        var pluginDirectory = Path.GetDirectoryName(pluginPath);
        var tempDirectory = Util.CreateDirectory(Path.Combine(Path.GetTempPath(), Name.Split(Path.GetInvalidFileNameChars()).Join("_"), Guid.NewGuid().ToString("N")));
        var inputPath = Path.Combine(tempDirectory, $"input.json");
        var outputPath = Path.Combine(tempDirectory, $"output.json");
        var loggerPath = Path.Combine(tempDirectory, $"logger.log");
        var progressPath = Path.Combine(tempDirectory, $"progress.log");
        File.WriteAllText(inputPath, task.Input.ToString(), Util.UTF8);
        //启动进度监控
        TaskCompletionSource StartListenProgress()
        {
            TaskCompletionSource progressCompletion = new();
            _ = Task.Run(async () =>
            {
                HashSet<string> records = [];
                FileSystemWatcher watcher = new();
                using SimpleMessageQueue<Json> messageQueue = new();
                messageQueue.Processer = async (msg) =>
                {
                    return await taskService.TaskCollection.UpdateProgress(taskID, msg);
                };
                watcher.Path = Path.GetDirectoryName(progressPath) ?? throw new NullReferenceException();
                watcher.Filter = Path.GetFileName(progressPath);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += (e, s) =>
                {
                    try
                    {
                        string[] progressLines;
                        using (FileStream fileStream = File.Open(progressPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            using StreamReader streamReader = new(fileStream, Util.UTF8);
                            progressLines = streamReader.ReadToEnd().Replace("\r", "").Split('\n');
                        }
                        foreach (var line in progressLines)
                        {
                            if (!line.Contains(' ')) continue;
                            var id = line[..line.IndexOf(' ')];
                            if (records.Contains(id))
                            {
                                continue;
                            }
                            var body = line[(line.IndexOf(' ') + 1)..];
                            if (Json.TryParse(body, out var progressJson))
                            {
                                records.Add(id);
                                Logger.Info($"Task {taskID} progress: {progressJson}");
                                messageQueue.Enqueue(progressJson);
                                _ = messageQueue.Notify();
                            }
                        }
                    }
                    catch
                    {

                    }
                };
                watcher.EnableRaisingEvents = true;
                await progressCompletion.Task;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                await messageQueue.Notify();
                records.Clear();
            });
            return progressCompletion;
        }
        TaskCompletionSource? progressCompletion = null;
        if (Entry.Contains("{Progress}") || Entry.Contains("{progress}"))
        {
            progressCompletion = StartListenProgress();
        }
        async Task processLogger(bool readErrorMessage)
        {
            if (File.Exists(loggerPath))
            {
                var pluginLogger = task.Trace.Target.GetOrCreateArray("PluginLogger");
                var lines = await File.ReadAllLinesAsync(loggerPath, Util.UTF8);
                if (readErrorMessage)
                {
                    var errorMessage = Util.GetErrorMessageFromLogger(lines);
                    if (errorMessage != null)
                    {
                        task.Trace.Error(errorMessage);
                    }
                }
                foreach (var line in lines)
                {
                    pluginLogger.Add(line);
                }
            }
        }
        async Task<ProcessResult> startProcess()
        {
            pluginDirectory = pluginDirectory?.Replace("\\", "/");
            inputPath = inputPath?.Replace("\\", "/");
            outputPath = outputPath?.Replace("\\", "/");
            loggerPath = loggerPath?.Replace("\\", "/");
            //执行插件时，可能是在服务中，也可能时在Agent中，所以需要根据情况替换
            var serverUrlPrefix = taskService.ShareServer.Enabled ? taskService.ShareServer.UrlPrefix : taskService.CurrentServerUrlPrefix;
            var cmd = entry
                .Replace("{.}", pluginDirectory).Replace("{Plugin}", pluginDirectory).Replace("{plugin}", pluginDirectory)
                .Replace("{Input}", inputPath).Replace("{input}", inputPath)
                .Replace("{Output}", outputPath).Replace("{output}", outputPath)
                .Replace("{Logger}", loggerPath).Replace("{logger}", loggerPath)
                .Replace("{Progress}", progressPath).Replace("{progress}", progressPath)
                .Replace("{Server}", serverUrlPrefix).Replace("{server}", serverUrlPrefix);
            task.Trace.Info("Plugin entry: " + cmd);
            var exitCode = await Util.CmdAsync(pluginDirectory, cmd, TimeSpan.FromMilliseconds(timeout));
            var outputIsValid = File.Exists(outputPath) && Json.TryParse(File.ReadAllText(outputPath, Util.UTF8), out _);
            return new()
            {
                ExitCode = exitCode,
                Success = outputIsValid,
                IsExistsOutput = File.Exists(outputPath),
                IsExistsLogger = File.Exists(loggerPath),
                IsExistsProgress = File.Exists(progressPath)
            };
        }
        int retryCount = 3;
        for (int i = 0; i < retryCount; i++)
        {
            var result = await startProcess();
            if (result.Success || i == retryCount - 1)
            {
                progressCompletion?.TrySetResult();
            }
            if (result.Success)
            {
                task.Output = Json.Parse(File.ReadAllText(outputPath, Util.UTF8));
                await processLogger(false);
                break;
            }
            else if (result.IsExistsOutput)
            {
                await processLogger(true);
                task.Trace.Error("Plugin output is invalid");
                break;
            }
            else if (result.ExitCode == 0)
            {
                await processLogger(true);
                task.Trace.Error("Plugin output is not exists");
                break;
            }
            else if (i == retryCount - 1)
            {
                await processLogger(true);
                task.Trace.Error("Plugin retry failed");
                break;
            }
            else if (result.ExitCode != 1)
            {
                await processLogger(true);
                task.Trace.Error($"Plugin exit with exception, code = {result.ExitCode}");
                break;
            }
            else
            {
                continue;
            }
        }
    }

    private async Task RunTypeSharp(TaskService taskService, TaskInterface task, bool useProgress)
    {
        var entry = Entry;
        var timeout = Timeout;
        var taskID = task.id;
        var pluginPath = taskService.PluginCollection.GetPluginPath(Name);
        var pluginDirectory = Path.GetDirectoryName(pluginPath);
        var tempDirectory = Util.CreateDirectory(Path.Combine(Path.GetTempPath(), Name.Split(Path.GetInvalidFileNameChars()).Join("_"), Guid.NewGuid().ToString("N")));
        var inputPath = Path.Combine(tempDirectory, $"input.json");
        var outputPath = Path.Combine(tempDirectory, $"output.json");
        var loggerPath = Path.Combine(tempDirectory, $"logger.log");
        var progressPath = Path.Combine(tempDirectory, $"progress.log");
        File.WriteAllText(inputPath, task.Input.ToString(), Util.UTF8);
        //启动进度监控
        TaskCompletionSource StartListenProgress()
        {
            TaskCompletionSource progressCompletion = new();
            _ = Task.Run(async () =>
            {
                HashSet<string> records = [];
                FileSystemWatcher watcher = new();
                watcher.Path = Path.GetDirectoryName(progressPath) ?? throw new NullReferenceException();
                watcher.Filter = Path.GetFileName(progressPath);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += (e, s) =>
                {
                    try
                    {
                        string[] progressLines;
                        using (FileStream fileStream = File.Open(progressPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            using StreamReader streamReader = new(fileStream, Util.UTF8);
                            progressLines = streamReader.ReadToEnd().Replace("\r", "").Split('\n');
                        }
                        foreach (var line in progressLines)
                        {
                            if (!line.Contains(' ')) continue;
                            var id = line[..line.IndexOf(' ')];
                            if (records.Contains(id))
                            {
                                continue;
                            }
                            records.Add(id);
                            var body = line[(line.IndexOf(' ') + 1)..];
                            if (Json.TryParse(body, out var progressJson))
                            {
                                Logger.Info($"Task {taskID} progress: {progressJson}");
                                _ = taskService.TaskCollection.UpdateProgress(taskID, progressJson);
                            }
                        }
                    }
                    catch
                    {

                    }
                };
                watcher.EnableRaisingEvents = true;
                await progressCompletion.Task;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                records.Clear();
            });
            return progressCompletion;
        }
        TaskCompletionSource? progressCompletion = null;
        if (useProgress)
        {
            progressCompletion = StartListenProgress();
        }
        async Task processLogger(bool readErrorMessage)
        {
            if (File.Exists(loggerPath))
            {
                var pluginLogger = task.Trace.Target.GetOrCreateArray("PluginLogger");
                var lines = await File.ReadAllLinesAsync(loggerPath, Util.UTF8);
                if (readErrorMessage)
                {
                    var errorMessage = Util.GetErrorMessageFromLogger(lines);
                    if (errorMessage != null)
                    {
                        task.Trace.Error(errorMessage);
                    }
                }
                foreach (var line in lines)
                {
                    pluginLogger.Add(line);
                }
            }
        }
        inputPath = inputPath?.Replace("\\", "/");
        outputPath = outputPath?.Replace("\\", "/");
        loggerPath = loggerPath?.Replace("\\", "/");
        //执行插件时，可能是在服务中，也可能时在Agent中，所以需要根据情况替换
        var serverUrlPrefix = taskService.ShareServer.Enabled ? taskService.ShareServer.UrlPrefix : taskService.CurrentServerUrlPrefix;
        entry = entry.Trim();
        var scriptPath = entry
            .Replace("{.}", pluginDirectory).Replace("{Plugin}", pluginDirectory).Replace("{plugin}", pluginDirectory);
        task.Trace.Info("Script path: " + scriptPath);
        var program = taskService.ProgramCollection.GetOrCreate(scriptPath);
        try
        {
            if (taskService.ProgramCollection.RunProgramByFilePathAndArgs == null)
            {
                throw new InvalidOperationException("ProgramFactory is null");
            }
            string[] args = ["--input", inputPath ?? "", "--output", outputPath ?? "", "--logger", loggerPath ?? "", "--progress", progressPath ?? ""];
            await taskService.ProgramCollection.RunProgramByFilePathAndArgs(program, scriptPath, args);
            if (File.Exists(outputPath))
            {
                try
                {
                    int retryCount = 0;
                    while (Util.IsFileInUse(outputPath))
                    {
                        await Task.Delay(10);
                        retryCount++;
                        if (retryCount > 100)
                        {
                            break;
                        }
                    }
                    task.Output = Json.Load(outputPath);
                    await processLogger(false);
                }
                catch (Exception e)
                {
                    await processLogger(true);
                    try
                    {
                        task.Trace.Error($"Plugin output is invalid, {File.ReadAllText(outputPath)}", e);
                    }
                    catch
                    {
                        task.Trace.Error($"Plugin output is invalid, read file failed.", e);
                    }
                }
            }
            else
            {
                await processLogger(true);
                task.Trace.Error("Plugin output is not exists");
            }
        }
        catch (Exception e)
        {
            await processLogger(false);
            var innerException = e.GetInnerException();
            var message = innerException?.Message ?? e.GetBaseException().Message;
            var lines = message.Replace("\r", "").Split('\n');
            if (lines.Length > 0)
            {
                message = lines[0];
            }
            Logger.Error(innerException ?? e);
            task.Trace.Error(message, innerException ?? e);
        }
        progressCompletion?.TrySetResult();
    }

    internal async Task<IDisposable> RunTypeSharpServer(TaskService taskService, object context)
    {
        var entry = Entry;
        var pluginPath = taskService.PluginCollection.GetPluginPath(Name);
        var pluginDirectory = Path.GetDirectoryName(pluginPath);
        var scriptPath = entry
            .Replace("{.}", pluginDirectory).Replace("{Plugin}", pluginDirectory).Replace("{plugin}", pluginDirectory);
        var program = taskService.ProgramCollection.GetOrCreate(scriptPath);
        try
        {
            if (taskService.ProgramCollection.RunProgramByFilePathAndContext == null)
            {
                throw new InvalidOperationException("ProgramFactory is null");
            }
            return await taskService.ProgramCollection.RunProgramByFilePathAndContext(program, scriptPath, context);
        }
        catch (Exception e)
        {
            Logger.Error($"TypeSharp server plugin `{Name}` run failed: {e.GetBaseException().Message}", e);
            throw;
        }
    }
}
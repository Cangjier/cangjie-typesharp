using Cangjie.Core.Exceptions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Steper;
using Cangjie.Dawn;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Tokens;
using Cangjie.Owners;
using Cangjie.TypeSharp.Steper;
using Cangjie.TypeSharp.System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TidyHPC.Common;
using TidyHPC.Extensions;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp;
public class TSProgram:IProgram
{
    public static TSScriptFileSystem FileSystem { get; } = new();

    private static ConcurrentDictionary<string, TextDocument> TextDocumentsCache { get; } = new();

    private static ConcurrentDictionary<string, TextContext> TextContextsCache { get; } = new();

    private static Dictionary<string,object> LockMap { get; } = new();

    private static object lockLockMap = new();

    private static object GetFileLock(string filePath)
    {
        lock (lockLockMap)
        {
            if (LockMap.TryGetValue(filePath, out var obj) == false)
            {
                obj = new object();
                LockMap[filePath] = obj;
            }
            return obj;
        }
    }

    public static void GetContext(string filePath,Owner owner,[NotNull]out TextDocument? document,[NotNull]out TextContext? textContext)
    {
        lock (GetFileLock(filePath))
        {
            if (TextDocumentsCache.TryGetValue(filePath, out document) == false)
            {
                var fileContent = FileSystem.GetFileContent(filePath);
                document = new(owner, fileContent);
                document.FilePath = filePath;
                TextDocumentsCache.TryAdd(filePath, document);
            }
            if (TextContextsCache.TryGetValue(filePath, out textContext) == false)
            {
                textContext = new(owner, TSScriptEngine.Template);
                textContext.Process(document);
                TextContextsCache.TryAdd(filePath, textContext);
            }
        }
    }

    /// <summary>
    /// 编译程序
    /// </summary>
    /// <param name="script"></param>
    /// <param name="filePath"></param>
    /// <param name="context"></param>
    public TSProgram(string filePath,string script,Context? context)
    {
        //Stopwatch stopwatch = new();
        //stopwatch.Start();
        //TextDocument = new(Owner, script);
        //TextDocument.FilePath = filePath;
        //TextContext = new(Owner, TSScriptEngine.Template);
        //TextContext.Process(TextDocument);
        //stopwatch.Stop();
        //Logger.Info($"Text Analyse: {stopwatch.ElapsedMilliseconds}ms");
        //stopwatch.Restart();
        //StepContext = new(Owner);
        //if (context != null)
        //{
        //    StepContext.MountVariableSpace(context.getContext);
        //}
        //var parseResult = TSScriptEngine.StepEngine.Parse(Owner, StepContext, TextContext.Root.Data, false);
        //Steps = parseResult.Steps;
        //stopwatch.Stop();
        //Logger.Info($"Compile: {stopwatch.ElapsedMilliseconds}ms");

        Stopwatch stopwatch = new();
        stopwatch.Start();
        TextDocuments = [];
        TextContexts = [];
        HashSet<string> filePathSet = [];
        StepContext = new(Owner);
        //stepContext.MountVariableSpace(context.getContext);
        Steps = new(Owner);
        void loadFile(string filePath)
        {
            if (filePathSet.Contains(filePath.ToLower())) return;
            if (File.Exists(filePath) == false)
            {
                Logger.Info($"File not found: {filePath}");
                return;
            }
            GetContext(filePath, Owner, out var document, out var textContext);
            stopwatch.Stop();
            Logger.Info($"Text Analyse: {stopwatch.ElapsedMilliseconds}ms, {filePath}");
            var imports = textContext.Root.Data.Where(item => item is Import).Select(item => (item as Import)!).ToArray();
            foreach (var import in imports)
            {
                var from = import.From;
                if (string.IsNullOrEmpty(from)) continue;
                if (from.Contains(".tsc")) continue;
                var fromFilePath = from;
                if (fromFilePath.EndsWith(".ts") == false)
                {
                    fromFilePath += "/index.ts";
                }
                fromFilePath = Path.GetFullPath(fromFilePath, Path.GetDirectoryName(filePath) ?? throw new Exception("filePath is null"));
                loadFile(fromFilePath);
            }
            TextDocuments.Add(document);
            TextContexts.Add(textContext);
        }
        loadFile(filePath);
        foreach (var textContext in TextContexts)
        {
            stopwatch.Restart();
            TSScriptEngine.StepEngine.Parse(Owner, StepContext, textContext.Root.Data, Steps, false);
            stopwatch.Stop();
            Logger.Info($"Compile: {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// 编译程序
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="script"></param>
    public TSProgram(string filePath, string script):this(filePath, script, null)
    {
    }

    /// <summary>
    /// 所有者
    /// </summary>
    public Owner Owner { get; } = new();

    /// <summary>
    /// 文本文档
    /// </summary>
    public List<TextDocument> TextDocuments { get; }

    /// <summary>
    /// 文本上下文
    /// </summary>
    public List<TextContext> TextContexts { get; }

    /// <summary>
    /// 步骤上下文
    /// </summary>
    public TSStepContext StepContext { get; }

    public Steps<char> Steps { get; }

    public async Task RunAsync(Context context)
    {
        ConcurrentDictionary<Type, RuntimeObject> contextObjects = new();
        using TSRuntimeContext runtimeContext = new();
        runtimeContext.ContextObjects = contextObjects;
        runtimeContext.MountVariableSpace(context.getContext);
        runtimeContext.AddContextObject(context);
        await Steps.RunAsync(runtimeContext);
        contextObjects.Clear();
    }

    public async Task<IDisposable> RunWithoutDisposeAsync(Context context)
    {
        ToDispose toDispose = new();
        ConcurrentDictionary<Type, RuntimeObject> contextObjects = new();
        TSRuntimeContext runtimeContext = new();
        runtimeContext.ContextObjects = contextObjects;
        runtimeContext.MountVariableSpace(context.getContext);
        runtimeContext.AddContextObject(context);
        await Steps.RunAsync(runtimeContext);
        // contextObjects.Clear();
        toDispose.Add(() =>
        {
            runtimeContext.Dispose();
            contextObjects.Clear();
        });
        return toDispose;
    }

    public void Dispose()
    {
        Owner.Release();
    }

    public static string GetExceptionMessage(Exception? e)
    {
        Exception? last = e;
        List<string> messages = [];
        while (last != null)
        {
            var inner = last.InnerException;
            if (last is RuntimeException<char> lastRuntimeException && inner is RuntimeException<char> innerRuntimeException)
            {
                if (lastRuntimeException.SourceRange == innerRuntimeException.SourceRange)
                {
                    last = last.InnerException;
                    continue;
                }
            }
            if (last is RuntimeException<char> lastRuntimeException2)
            {
                if (last.InnerException == null)
                {
                    messages.Add(last.Message);
                }
                else
                {
                    messages.Add(lastRuntimeException2.ScriptTrace);
                }
            }
            else
            {
                messages.Add(last.Message);
            }
            last = last.InnerException;
        }
        messages.Reverse();
        return messages.Join("\r\n");
    }
}

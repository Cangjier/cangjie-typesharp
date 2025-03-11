using Cangjie.Core.Exceptions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Steper;
using Cangjie.Dawn;
using Cangjie.Dawn.Text;
using Cangjie.Owners;
using Cangjie.TypeSharp.Steper;
using Cangjie.TypeSharp.System;
using System.Collections.Concurrent;
using System.Diagnostics;
using TidyHPC.Extensions;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp;
public class TSProgram:IProgram
{
    /// <summary>
    /// 编译程序
    /// </summary>
    /// <param name="script"></param>
    /// <param name="filePath"></param>
    /// <param name="context"></param>
    public TSProgram(string filePath,string script,Context? context)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        TextDocument = new(Owner, script);
        TextDocument.FilePath = filePath;
        TextContext = new(Owner, TSScriptEngine.Template);
        TextContext.Process(TextDocument);
        stopwatch.Stop();
        Logger.Info($"Text Analyse: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Restart();
        StepContext = new(Owner);
        if (context != null)
        {
            StepContext.MountVariableSpace(context.getContext);
        }
        var parseResult = TSScriptEngine.StepEngine.Parse(Owner, StepContext, TextContext.Root.Data, false);
        Steps = parseResult.Steps;
        stopwatch.Stop();
        Logger.Info($"Compile: {stopwatch.ElapsedMilliseconds}ms");
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
    public TextDocument TextDocument { get; }

    /// <summary>
    /// 文本上下文
    /// </summary>
    public TextContext TextContext { get; }

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

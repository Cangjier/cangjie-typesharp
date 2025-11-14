using Cangjie.Core.Extensions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Syntax.Templates;
using Cangjie.Owners;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Tokens.String;
using TidyHPC.LiteJson;
using Cangjie.TypeSharp.Steper;
using Cangjie.Core.NativeOperatorMethods;
using System.Diagnostics;
using TidyHPC.Loggers;
using Cangjie.TypeSharp.System;
using Cangjie.Core.Steper;
using System.Reflection;
using Cangjie.Core.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Xml.Linq;
using Cangjie.Core;
using Cangjie.Dawn.Text.Tokens;
using System.Configuration;

namespace Cangjie.TypeSharp;


public class TSScriptFileSystem
{
    public Func<string,bool> Exists { get; set; } = (string file) => File.Exists(file);

    public Func<string, Task<string>> GetFileContentAsync { get; set; } = async (string file) => await File.ReadAllTextAsync(file, Util.UTF8);

    public Func<string,string> GetFileContent { get; set; } = (string file) => File.ReadAllText(file, Util.UTF8);
}

public class TSScriptEngine
{
    static TSScriptEngine()
    {
        Json._op_Invoke = (self, name, args) =>
        {
            var value = self.Get(name, Json.Null);
            if (value.IsNull)
            {
                object? instance = null;
                Type? instanceType = null;
                if (self.IsNull) throw new Exception("The instance is null.");
                if (self.Is<RuntimeObject>())
                {
                    var objectValue = self.As<RuntimeObject>();
                    instance = objectValue.Value;
                    instanceType = objectValue.Type;
                }
                else
                {
                    instance = self.Node;
                    instanceType = self.Node?.GetType();
                }
                if (instanceType == null) throw new Exception("The instance type is null.");
                var inputTypes = args.Select(arg => typeof(Json)).ToArray();
                if (instanceType.TryFindInstanceMethod([name], x => true, inputTypes.IsAssignTo, out var methodInfo))
                {
                    if(inputTypes.TryAssignTo(args.Select(item=>(object)item).ToArray(), methodInfo.GetParameters(), out object?[] parameters))
                    {
                        return new Json(methodInfo.Invoke(instance, parameters));
                    }
                    else
                    {
                        throw new Exception($"The parameters of {name} is not match.");
                    }
                }
                if (args.All(item => item.Is<RuntimeObject>()))
                {
                    inputTypes = args.Select(arg => arg.As<RuntimeObject>().Type).ToArray();
                    var inputValues = args.Select(arg => arg.As<RuntimeObject>().Value).ToArray();
                    if (instanceType.TryFindInstanceMethod([name], x => true, inputTypes.IsAssignTo, out methodInfo))
                    {
                        if(inputTypes.TryAssignTo(inputValues, methodInfo.GetParameters(), out var parameters))
                        {
                            return new Json(methodInfo.Invoke(instance, parameters));
                        }
                        else
                        {
                            throw new Exception($"The parameters of {name} is not match.");
                        }
                    }
                    else if (instanceType.TryFindInstanceField(name, out var fieldInfo))
                    {
                        if (fieldInfo.FieldType.BaseType == typeof(MulticastDelegate))
                        {
                            var delegateInstance = fieldInfo.GetValue(instance);
                            if (delegateInstance is Delegate @delegate)
                            {
                                return new Json(@delegate.DynamicInvoke(inputValues));
                            }
                            else
                            {
                                throw new Exception($"The field {name} is not a delegate.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"The parameters of {name} is not match.");
                    }
                }
                throw new Exception($"The function {name} is not found.");
            }
            else
            {
                Delegate? @delegate = null;
                if (value.Is<RuntimeObject>())
                {
                    var objectValue = value.As<RuntimeObject>();
                    if (objectValue.Value is Delegate)
                    {
                        @delegate = (Delegate)objectValue.Value;
                    }
                }
                else if (value.Is<Delegate>())
                {
                    @delegate = value.As<Delegate>();
                }
                if (@delegate != null)
                {
                    var parameters = @delegate.Method.GetParameters();
                    var inputTypes = args.Select(arg => typeof(Json)).ToArray();
                    if (inputTypes.TryAssignTo(args.Select(item => (object)item).ToArray(), parameters, out var inputValues))
                    {
                        try
                        {
                            return new Json(@delegate.DynamicInvoke(inputValues));
                        }
                        catch(Exception e)
                        {
                            if(e.InnerException is RuntimeException<char> runtimeException)
                            {
                                throw new RuntimeException<char>(runtimeException);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"The parameters of {name} is not match.");
                    }
                }
                else
                {
                    throw new Exception($"The function {name} is not found.");
                }
            }
        };
        Json._Invoke = (self, args) =>
        {
            Delegate? @delegate = null;
            if (self.Is<RuntimeObject>())
            {
                var objectValue = self.As<RuntimeObject>();
                if (objectValue.Value is Delegate)
                {
                    @delegate = (Delegate)objectValue.Value;
                }
            }
            else if (self.Is<Delegate>())
            {
                @delegate = self.As<Delegate>();
            }
            if (@delegate != null)
            {
                var inputTypes = args.Select(arg => typeof(Json)).ToArray();
                var methodInfo = @delegate.Method;
                var inputValues = args.Select(item => (object)item).ToArray();
                if (inputTypes.TryAssignTo(inputValues, methodInfo.GetParameters(), out var parameters))
                {
                    return new Json(@delegate.DynamicInvoke([.. parameters]));
                    //return new Json(methodInfo.Invoke(instance, parameters));
                }
                else
                {
                    throw new Exception($"The parameters is not match.");
                }
            }
            else
            {
                throw new Exception($"The function is not found.");
            }
        };
        BooleanOperatorMethods.OnLogicalNot = value =>
        {
            if (value is bool valueBoolean) return !valueBoolean;
            else if (value is Json valueJson)
            {
                if (valueJson.IsNull || valueJson.IsUndefined) return true;
                if (valueJson.IsBoolean) return !valueJson.IsBoolean;
                if (valueJson.IsNumber) return valueJson.AsNumber == 0;
                return false;
            }
            else if (value is int valueInt32) return valueInt32 == 0;
            else if (value is long valueInt64) return valueInt64 == 0;
            else if (value is float valueSingle) return valueSingle == 0;
            else if (value is double valueDouble) return valueDouble == 0;
            else if (value is decimal valueDecimal) return valueDecimal == 0;
            else if (value is string valueString) return string.IsNullOrEmpty(valueString);
            else if (value is char valueChar) return valueChar == '\0';
            else return false;
        };
        ObjectOperatorMethods._op_ImplicitTo = (from, toType) =>
        {
            if (from is Json fromJson) return Json.op_ImplicitTo(fromJson, toType);
            else return from;
        };
        Json.ImplicitTo = (from, toType) =>
        {
            var fromType = from.Node?.GetType();
            if (fromType == null || from.Node is null)
            {
                return from.Node;
            }
            else if (DelegateUtil.IsCanConvertDelegate(fromType, toType))
            {
                return DelegateUtil.ConvertDelegate(from.Node, fromType, toType);
            }
            else
            {
                return from.Node;
            }
        };
    }

    public static Template<char> Template { get; } = InitialTemplate(new());

    public static TSStepEngine StepEngine { get; } = new();

    public static Template<char> InitialTemplate(Template<char> template)
    {
        template.BranchTemplate.AddModifyItem(typeof(StringGuide.Branch), branch =>
        {
            ((StringGuide.Branch)branch).AddStringChar('`');
            ((StringGuide.Branch)branch).AddStringChar('\'');
        });
        template.SymbolTemplate.Ban('_');
        return template;
    }

    public static Json Run(string script,Context context,Action<TSStepContext>? onStepContext,Action<TSRuntimeContext>? onRuntimeContext)
    {
        using Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
#if DEBUG
        var root = textContext.Root.ToString();
        Console.WriteLine(Path.GetFullPath("root.xml"));
        File.WriteAllText("root.xml",root);
#endif

        TSStepContext stepContext = new(owner);
        stepContext.MountVariableSpace(context.getContext);
        onStepContext?.Invoke(stepContext);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;
#if DEBUG
        Console.WriteLine(Path.GetFullPath("steps.text"));
        File.WriteAllText($"steps.text", steps.ToString());
#endif
        using TSRuntimeContext runtimeContext = new();
        runtimeContext.MountVariableSpace(context.getContext);
        runtimeContext.ContextObjects = new();
        runtimeContext.AddContextObject(context);
        onRuntimeContext?.Invoke(runtimeContext);
        steps.Run(runtimeContext);
        var lastValue = runtimeContext.GetLastObject().Value;
        runtimeContext.ContextObjects.Clear();
        return new(lastValue);
    }

    public static async Task<Json> RunAsync(string filePath,string script, Context context, Action<TSStepContext>? onStepContext, Action<TSRuntimeContext>? onRuntimeContext)
    {
        var fileSystem = new TSScriptFileSystem();
        fileSystem.GetFileContentAsync = async (string file) =>
        {
            if (file == filePath)
            {
                return script;
            }
            else
            {
                return await File.ReadAllTextAsync(file, Util.UTF8);
            }
        };
        fileSystem.GetFileContent = (string file) =>
        {
            if (file == filePath)
            {
                return script;
            }
            else
            {
                return File.ReadAllText(file, Util.UTF8);
            }
        };
        fileSystem.Exists = (string file) =>
        {
            if (file == filePath)
            {
                return true;
            }
            else
            {
                return File.Exists(file);
            }
        };
        return await RunAsyncWithFiles(filePath, fileSystem, context, onStepContext, onRuntimeContext);
    }

    public static async Task<Json> RunAsyncWithFiles(string filePath,TSScriptFileSystem fileSystem, Context context, Action<TSStepContext>? onStepContext, Action<TSRuntimeContext>? onRuntimeContext)
    {
        using Owner owner = new();
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<TextDocument> documents = [];
        List<TextContext> textContexts = [];
        HashSet<string> filePathSet = [];
        TSStepContext stepContext = new(owner);
        stepContext.MountVariableSpace(context.getContext);
        onStepContext?.Invoke(stepContext);
        Steps<char> steps = new(owner);
        async Task loadFile(string filePath)
        {
            if(filePathSet.Contains(filePath.ToLower())) return;
            if (fileSystem.Exists(filePath) == false)
            {
                Logger.Info($"File not found: {filePath}");
                return;
            }
            var fileContent = await fileSystem.GetFileContentAsync(filePath);
            TextDocument document = new(owner, fileContent);
            document.FilePath = filePath;
            TextContext textContext = new(owner, Template);
            textContext.Process(document);
            stopwatch.Stop();
            Logger.Info($"Text Analyse: {stopwatch.ElapsedMilliseconds}ms, {filePath}");
            var imports = textContext.Root.Data.Where(item => item is Import).Select(item => (item as Import)!).ToArray();
            foreach(var import in imports)
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
                await loadFile(fromFilePath);
            }
            documents.Add(document);
            textContexts.Add(textContext);
#if DEBUG
            //var root = textContext.Root.ToString();
            //Console.WriteLine(Path.GetFullPath("root.xml"));
            //File.WriteAllText("root.xml", root);
#endif
        }
        await loadFile(filePath);
        foreach (var textContext in textContexts)
        {
            stopwatch.Restart();
            StepEngine.Parse(owner, stepContext, textContext.Root.Data, steps, false);
            stopwatch.Stop();
            Logger.Info($"Compile: {stopwatch.ElapsedMilliseconds}ms");
#if DEBUG
            //Console.WriteLine(Path.GetFullPath("steps.text"));
            //File.WriteAllText($"steps.text", steps.ToString());
#endif
        }

        using TSRuntimeContext runtimeContext = new();
        runtimeContext.MountVariableSpace(context.getContext);
        runtimeContext.ContextObjects = new();
        runtimeContext.AddContextObject(context);
        onRuntimeContext?.Invoke(runtimeContext);
        await steps.RunAsync(runtimeContext);
        var lastValue = runtimeContext.GetLastObject().Value;
        runtimeContext.ContextObjects.Clear();
        return new(lastValue);
    }

    public static Json Run(string script, Context context) => Run(script,context, null, null);

    public static async Task<Json> RunAsync(string filePath, string script, Context context) => await RunAsync(filePath, script, context, null, null);

}

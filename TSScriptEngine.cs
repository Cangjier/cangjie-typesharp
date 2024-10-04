using Cangjie.Core.Extensions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Syntax.Templates;
using Cangjie.Imp.Text;
using Cangjie.Imp.Text.Units.String;
using Cangjie.TypeSharp.Steper;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp;
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
                    else
                    {
                        throw new Exception($"The parameters of {name} is not match.");
                    }
                }
                throw new Exception($"The function {name} is not found.");
            }
            else
            {
                CallInstance<char>? callInstanceNullable = null;
                Delegate? @delegate = null;
                if (value.Is<RuntimeObject>())
                {
                    var objectValue = value.As<RuntimeObject>();
                    if (objectValue.Value is CallInstance<char>)
                    {
                        callInstanceNullable = (CallInstance<char>)objectValue.Value;
                    }
                    else if (objectValue.Value is Delegate)
                    {
                        @delegate = (Delegate)objectValue.Value;
                    }
                }
                else if (value.Is<CallInstance<char>>())
                {
                    callInstanceNullable = value.As<CallInstance<char>>();
                }
                else if (value.Is<Delegate>())
                {
                    @delegate = value.As<Delegate>();
                }
                if (callInstanceNullable != null)
                {
                    var callInstance = callInstanceNullable.Value;
                    if (callInstance.CallInterface.ParameterCount != args.Length)
                    {
                        throw new Exception($"The number of parameters of {name} is not equal to {args.Length}.");
                    }
                    RuntimeObject[] objects = new RuntimeObject[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        objects[i] = new RuntimeObject()
                        {
                            Type = callInstance.CallInterface.ParameterTypes[i],
                            Value = args[i].Node
                        };
                    }
                    return new Json(callInstance.Invoke(objects).Value);
                }
                else if (@delegate != null)
                {
                    return new Json(@delegate.DynamicInvoke([.. args.Select(arg => arg.Node)]));
                }
                else
                {
                    throw new Exception($"The function {name} is not found.");
                }
            }
        };
        Json._Invoke = (self, args) =>
        {
            CallInstance<char>? callInstanceNullable = null;
            Delegate? @delegate = null;
            if (self.Is<RuntimeObject>())
            {
                var objectValue = self.As<RuntimeObject>();
                if (objectValue.Value is CallInstance<char>)
                {
                    callInstanceNullable = (CallInstance<char>)objectValue.Value;
                }
                else if (objectValue.Value is Delegate)
                {
                    @delegate = (Delegate)objectValue.Value;
                }
            }
            else if (self.Is<CallInstance<char>>())
            {
                callInstanceNullable = self.As<CallInstance<char>>();
            }
            else if (self.Is<Delegate>())
            {
                @delegate = self.As<Delegate>();
            }
            if (callInstanceNullable != null)
            {
                var callInstance = callInstanceNullable.Value;
                if (callInstance.CallInterface.ParameterCount != args.Length)
                {
                    throw new Exception($"The number of parameters is not equal to {args.Length}.");
                }
                RuntimeObject[] objects = new RuntimeObject[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    objects[i] = new RuntimeObject()
                    {
                        Type = callInstance.CallInterface.ParameterTypes[i],
                        Value = callInstance.CallInterface.ParameterTypes[i] == typeof(Json) ? args[i] : args[i].Node
                    };
                }
                return new Json(callInstance.Invoke(objects).Value);
            }
            else if (@delegate != null)
            {
                return new Json(@delegate.DynamicInvoke([.. args.Select(arg => arg.Node)]));
            }
            else
            {
                throw new Exception($"The function is not found.");
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

    public static Json Run(string script,Action<TSStepContext>? onStepContext,Action<TSRuntimeContext>? onRuntimeContext)
    {
        Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
        var root = textContext.Root.ToString();
        TSStepContext stepContext = new(owner);
        onStepContext?.Invoke(stepContext);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;

        TSRuntimeContext runtimeContext = new(owner);
        onRuntimeContext?.Invoke(runtimeContext);
        steps.Run(runtimeContext);
        var lastValue = runtimeContext.GetLastObject().Value;
        owner.Release();
        return new(lastValue);
    }

    public static async Task<Json> RunAsync(string script, Action<TSStepContext>? onStepContext, Action<TSRuntimeContext>? onRuntimeContext)
    {
        Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
        var root = textContext.Root.ToString();
        TSStepContext stepContext = new(owner);
        onStepContext?.Invoke(stepContext);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;

        TSRuntimeContext runtimeContext = new(owner);
        onRuntimeContext?.Invoke(runtimeContext);
        await steps.RunAsync(runtimeContext);
        var lastValue = runtimeContext.GetLastObject().Value;
        owner.Release();
        return new(lastValue);
    }

    public static Json Run(string script) => Run(script, null, null);

    public static async Task<Json> RunAsync(string script) => await RunAsync(script, null, null);
}

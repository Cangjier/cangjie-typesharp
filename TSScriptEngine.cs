using Cangjie.Core.Extensions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Syntax.Templates;
using Cangjie.Owners;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Units.String;
using TidyHPC.LiteJson;
using Cangjie.TypeSharp.Steper;
using Cangjie.Core.NativeOperatorMethods;

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
                return new Json(@delegate.DynamicInvoke([.. args.Select(arg => arg.Node)]));
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
#if DEBUG
        var root = textContext.Root.ToString();
        Console.WriteLine(Path.GetFullPath("root.xml"));
        File.WriteAllText("root.xml",root);
#endif

        TSStepContext stepContext = new(owner);
        onStepContext?.Invoke(stepContext);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;
#if DEBUG
        Console.WriteLine(Path.GetFullPath("steps.text"));
        File.WriteAllText($"steps.text", steps.ToString());
#endif
        TSRuntimeContext runtimeContext = new();
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
#if DEBUG
        var root = textContext.Root.ToString();
        Console.WriteLine(Path.GetFullPath("root.xml"));
        File.WriteAllText("root.xml", root);
#endif
        TSStepContext stepContext = new(owner);
        onStepContext?.Invoke(stepContext);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;
#if DEBUG
        Console.WriteLine(Path.GetFullPath("steps.text"));
        File.WriteAllText($"steps.text", steps.ToString());
#endif
        TSRuntimeContext runtimeContext = new();
        onRuntimeContext?.Invoke(runtimeContext);
        await steps.RunAsync(runtimeContext);
        var lastValue = runtimeContext.GetLastObject().Value;
        owner.Release();
        return new(lastValue);
    }

    public static Json Run(string script) => Run(script, null, null);

    public static async Task<Json> RunAsync(string script) => await RunAsync(script, null, null);
}

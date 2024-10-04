using Cangjie.Core.Runtime;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp;

public class TSRuntimeContext : RuntimeContext<char>
{
    public TSRuntimeContext(IOwner owner) : this(owner, [new TSScope() { Type = ScopeType.Return }])
    {

    }

    public TSRuntimeContext(IOwner owner, TSScope[] scopes):base(owner)
    {
        foreach (var item in scopes)
        {
            StackVariableSpace.Push(item);
        }
    }

    public bool IsSupportDefaultField { get; set; } = false;

    public Json MountedVariableSpace { get; private set; } = Json.Null;

    public void MountVariableSpace(Json value)
    {
        IsSupportDefaultField = true;
        MountedVariableSpace = value;
    }

    public override bool IsConditionTrue(RuntimeObject value)
    {
        if (value.Value is false) return false;
        else if (value.Value is true) return true;
        else if (value.Value is Json jsonValue)
        {
            if (jsonValue.IsTrue) return true;
            else if (jsonValue.IsFalse) return false;
            else if (jsonValue.IsUndefined) return false;
            else
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsConditionNull(RuntimeObject value)
    {
        if (value.Value is null) return true;
        else if (value.Value is Json jsonValue)
        {
            return jsonValue.IsNull || jsonValue.IsUndefined;
        }
        else if (value.Value is string valueString) return Json.Undefined == valueString;
        return false;
    }

    public static string AnyToString(object? value)
    {
        if (value is null) return "null";
        else if (value is string valueString) return valueString;
        // 判断value.Value是否是Json类型
        else if (value is Json jsonValue)
        {
            return jsonValue.ToString();
        }
        // 判断value.Value的ToString是否重载
        else if (value.GetType().GetMethod("ToString", Type.EmptyTypes)?.DeclaringType != typeof(object))
        {
            return value.ToString() ?? "";
        }
        else return new Json(value).ToString();
    }

    public override string ToString(RuntimeObject value)
    {
        return AnyToString(value.Value);
    }

    public Stack<TSScope> StackVariableSpace { get; } = new();

    public override void BeginCommonScope()
    {
        StackVariableSpace.Push(new()
        {
            Type = ScopeType.Common
        });
    }

    public override void BeginLoopScope()
    {
        StackVariableSpace.Push(new()
        {
            Type = ScopeType.Loop
        });
    }

    public override void BeginReturnScope()
    {
        StackVariableSpace.Push(new()
        {
            Type = ScopeType.Return
        });
    }

    public override void EndCommonScope()
    {
        StackVariableSpace.Pop();
    }

    public override void EndLoopScope()
    {
        while (true)
        {
            if(StackVariableSpace.TryPeek(out var scope))
            {
                if (scope.Type == ScopeType.Loop)
                {
                    StackVariableSpace.Pop();
                    break;
                }
                else
                {
                    StackVariableSpace.Pop();
                }
            }
            else
            {
                break;
            }
        }
    }

    public override void EndReturnScope()
    {
        while (true)
        {
            if(StackVariableSpace.TryPeek(out var scope))
            {
                if (scope.Type == ScopeType.Return)
                {
                    StackVariableSpace.Pop();
                    break;
                }
                else
                {
                    StackVariableSpace.Pop();
                }
            }
            else
            {
                break;
            }
        }
    }

    public override void BeginTryScope()
    {
        StackVariableSpace.Push(new()
        {
            Type = ScopeType.Try
        });
    }

    public override void EndTryScope()
    {
        while (true)
        {
            if (StackVariableSpace.TryPeek(out var scope))
            {
                if (scope.Type == ScopeType.Try)
                {
                    StackVariableSpace.Pop();
                    break;
                }
                else
                {
                    StackVariableSpace.Pop();
                }
            }
            else
            {
                break;
            }
        }
    }

    public override bool ContainsVariable(string key)
    {
        foreach (var item in StackVariableSpace)
        {
            if (item.Variables.ContainsKey(key))
            {
                return true;
            }
        }
        if(IsSupportDefaultField && MountedVariableSpace.ContainsKey(key))
        {
            return true;
        }
        return false;
    }

    public override RuntimeObject GetVariable(string key)
    {
        foreach (var item in StackVariableSpace)
        {
            if (item.Variables.TryGetValue(key, out RuntimeObject value))
            {
                return value;
            }
        }
        if (IsSupportDefaultField)
        {
            return new()
            {
                Type = typeof(Json),
                Value = MountedVariableSpace[key]
            };
        }
        return new()
        {
            Type = typeof(Json),
            Value = Json.Undefined
        };
    }

    public override RuntimeContext<char> RegitserVariable(string key, RuntimeObject value)
    {
        StackVariableSpace.Peek().Variables[key] = value;
        return this;
    }

    public override RuntimeContext<char> UpdateVariable(string key, RuntimeObject value)
    {
        foreach (var item in StackVariableSpace)
        {
            if (item.Variables.ContainsKey(key))
            {
                item.Variables[key] = value;
                return this;
            }
        }
        throw new Exception($"Variable {key} not found.");
    }

    public override void Release()
    {
        base.Release();
    }

    public override RuntimeContext<char> Clone()
    {
        var context = new TSRuntimeContext(Owner, []);
        context.StackVariableSpace.Clear();
        foreach (var item in StackVariableSpace)
        {
            context.StackVariableSpace.Push(item);
        }
        return context;
    }
}


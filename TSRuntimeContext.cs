using Cangjie.Core.Runtime;
using Cangjie.Owners;
using TidyHPC.LiteJson;
using RuntimeObject = Cangjie.Core.Runtime.RuntimeObject;

namespace Cangjie.TypeSharp;

public class TSRuntimeContext : RuntimeContext<char>
{
    public TSRuntimeContext() : this([new RuntimeScope() { Type = RuntimeScopeType.Return }])
    {

    }

    public TSRuntimeContext(RuntimeScope[] scopes):base()
    {
        foreach (var item in scopes)
        {
            StackScope.Push(item);
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
            else if (jsonValue.IsNull) return false;
            else if (jsonValue.IsString)
            {
                return jsonValue.AsString.Length != 0;
            }
            else if (jsonValue.IsNumber)
            {
                return jsonValue.AsNumber != 0;
            }
            else
            {
                return true;
            }
        }
        else if (value.Value is null) return false;
        else if (value.Value is string valueString)
        {
            if(valueString.Length == 0) return false;
            return Json.Undefined != valueString;
        }
        else if(value.Value is int valueInt)return valueInt != 0;
        else if (value.Value is double valueDouble) return valueDouble != 0;
        else if (value.Value is float valueFloat) return valueFloat != 0;
        else if (value.Value is long valueLong) return valueLong != 0;
        else if (value.Value is short valueShort) return valueShort != 0;
        else if (value.Value is byte valueByte) return valueByte != 0;
        else if (value.Value is sbyte valueSbyte) return valueSbyte != 0;
        else if (value.Value is uint valueUint) return valueUint != 0;
        else if (value.Value is ulong valueUlong) return valueUlong != 0;
        else if (value.Value is ushort valueUshort) return valueUshort != 0;
        else if (value.Value is decimal valueDecimal) return valueDecimal != 0;
        else return true;
    }

    public override bool IsConditionNull(RuntimeObject value)
    {
        if (value.Value is null) return true;
        else if (value.Value is Json jsonValue)
        {
            return jsonValue.IsNull || jsonValue.IsUndefined;
        }
        else if (value.Value is string valueString) return Json.Undefined == valueString || valueString.Length == 0;
        else if (value.Value is int valueInt) return valueInt == 0;
        else if (value.Value is double valueDouble) return valueDouble == 0;
        else if (value.Value is float valueFloat) return valueFloat == 0;
        else if (value.Value is long valueLong) return valueLong == 0;
        else if (value.Value is short valueShort) return valueShort == 0;
        else if (value.Value is byte valueByte) return valueByte == 0;
        else if (value.Value is sbyte valueSbyte) return valueSbyte == 0;
        else if (value.Value is uint valueUint) return valueUint == 0;
        else if (value.Value is ulong valueUlong) return valueUlong == 0;
        else if (value.Value is ushort valueUshort) return valueUshort == 0;
        else if (value.Value is decimal valueDecimal) return valueDecimal == 0;
        else return false;
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

    public override bool ContainsVariable(string key)
    {
        if (base.ContainsVariable(key))
        {
            return true;
        }
        if(IsSupportDefaultField && MountedVariableSpace.ContainsKey(key))
        {
            return true;
        }
        return false;
    }

    public override RuntimeObject GetVariable(string key)
    {
        foreach (var item in StackScope)
        {
            if (item.TryGetValue(key, out RuntimeObject value))
            {
                return value;
            }
        }
        if(CatchScope.TryGetValue(key, out RuntimeObject catchValue))
        {
            return catchValue;
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

    public override object? GetAwaitTask(object? value)
    {
        if (value is Json jsonValue)
        {
            return jsonValue.Node;
        }
        else return null;
    }

    public override object? GetAwaitResult(object? value)
    {
        if (value is null) throw new NullReferenceException();
        var resultProperty = value.GetType().GetProperty("Result");
        if (resultProperty == null) return null;
        return resultProperty.GetValue(value);
    }

    public override RuntimeContext<char> CatchClone()
    {
        var context = new TSRuntimeContext([]);
        CatchScope.CopyTo(context.CatchScope);
        return context;
    }

    public override RuntimeContext<char> CatchClone(List<string> catchFields)
    {
        var context = new TSRuntimeContext([]);
        HashSet<string> fields = [.. catchFields];
        foreach (var item in StackScope)
        {
            foreach(var catchField in fields.ToArray())
            {
                if (item.Variables.TryGetValue(catchField,out var runtimeVariable))
                {
                    context.CatchScope.Variables.Add(catchField, runtimeVariable);
                    fields.Remove(catchField);
                }
            }
            if(fields.Count == 0)
            {
                break;
            }
        }
        if (fields.Count != 0)
        {
            foreach(var catchField in fields.ToArray())
            {
                if(CatchScope.Variables.TryGetValue(catchField,out var runtimeVariable))
                {
                    context.CatchScope.Variables.Add(catchField, runtimeVariable);
                    fields.Remove(catchField);
                }
            }
            if (fields.Count != 0)
            {
                throw new Exception("Catch fields not found.");
            }
        }
        return context;
    }
}


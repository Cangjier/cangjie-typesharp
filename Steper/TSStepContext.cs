using Cangjie.Core.Steper;
using Cangjie.Owners;
using TidyHPC.LiteJson;
using Cangjie.TypeSharp.BasicType;
using Cangjie.TypeSharp.System;
using Cangjie.TypeSharp.FullNameScript;
using TidyHPC.LiteXml;
using System.Text;
using System.Text.RegularExpressions;

namespace Cangjie.TypeSharp.Steper;
public class TSStepContext : StepContext<char>
{
    public TSStepContext(IOwner owner) : base(owner)
    {
        TypeInference.BasicTypes = [typeof(Json),typeof(JsonWrapper)];
        TypeInference.InferenceType = fullName =>
        {
            if (fullName == "string") return typeof(string);
            else if (fullName == "number") return typeof(double);
            else if (fullName == "any") return typeof(Json);
            else if (fullName == "boolean") return typeof(bool);
            else if (fullName == "void") return typeof(void);
            else if (fullName.EndsWith("[]"))
            {
                fullName = fullName.Substring(0, fullName.Length - 2);
                return typeof(Json);
            }
            else return typeof(Json);
        };
        UsingNamespaces.Add(typeof(console).Namespace ?? throw new NullReferenceException($"{nameof(console)}.Namespace"));
        UsingNamespaces.Add(typeof(FullName).Namespace ?? throw new NullReferenceException($"{nameof(FullName)}.Namespace"));
        UsingNamespaces.Add(typeof(Json).Namespace ?? throw new NullReferenceException($"{nameof(Json)}.Namespace"));
        UsingNamespaces.Add(typeof(Xml).Namespace ?? throw new NullReferenceException($"{nameof(Xml)}.Namespace"));
        UsingNamespaces.Add(typeof(UTF8Encoding).Namespace ?? throw new NullReferenceException($"{nameof(UTF8Encoding)}.Namespace"));
        UsingNamespaces.Add(typeof(Regex).Namespace ?? throw new NullReferenceException($"{nameof(Regex)}.Namespace"));
        UsingNamespaces.Add(typeof(Task).Namespace ?? throw new NullReferenceException($"{nameof(Task)}.Namespace"));
        UsingNamespaces.AddRange(["System","System.IO"]);
        ContextTypes.Add(typeof(context));
        TypeInference.MixType = types =>
        {
            // 如果所有types相同，返回types[0]
            if (types.Distinct().Count() == 1) return types[0];
            else if (types.All(item => item == typeof(int) || item == typeof(float) || item == typeof(double)))
            {
                if (types.Contains(typeof(double))) return typeof(double);
                else if (types.Contains(typeof(float))) return typeof(float);
                else return typeof(int);
            }
            else return typeof(Json);
        };
    }

    public bool IsSupportDefaultField { get; set; } = false;

    public Json MountedVariableSpace { get; private set; } = Json.Null;

    public void MountVariableSpace(Json value)
    {
        IsSupportDefaultField = true;
        MountedVariableSpace = value;
    }

    public override bool TryGetField(string name, out StepMetaData<char>? fieldMeta)
    {
        if(base.TryGetField(name, out fieldMeta)) return true;
        if (IsSupportDefaultField && MountedVariableSpace.ContainsKey(name))
        {
            fieldMeta = typeof(Json);
            return true;
        }
        return false;
    }

    public override Type? GetAwaitTaskType(Type taskType)
    {
        if (taskType == typeof(Json))
        {
            return typeof(Task<object>);
        }
        else
        {
            return null;
        }
    }
}
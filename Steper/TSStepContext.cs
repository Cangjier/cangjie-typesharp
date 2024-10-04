using Cangjie.Core.Steper;
using OwnerInterface;
using TidyHPC.LiteJson;
using TypeSharp.BasicType;
using TypeSharp.System;

namespace TypeSharp.Steper;
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
        UsingNamespaces.AddRange(["System","System.IO"]);
        ContextTypes.Add(typeof(context));
        TypeInference.MixType = types =>
        {
            if (types.All(item => item == typeof(int) || item == typeof(float) || item == typeof(double)))
            {
                if (types.Contains(typeof(double))) return typeof(double);
                else if (types.Contains(typeof(float))) return typeof(float);
                else return typeof(int);
            }
            else return typeof(Json);
        };
    }

    public bool IsSupportDefaultField { get; set; } = false;

    public override bool TryGetField(string name, out StepMetaData<char>? fieldMeta)
    {
        if(base.TryGetField(name, out fieldMeta)) return true;
        if(IsSupportDefaultField)
        {
            fieldMeta = typeof(Json);
            return true;
        }
        return false;
    }
}
using Cangjie.Core.Runtime;

namespace Cangjie.TypeSharp;

public enum ScopeType
{
    Common,
    Loop,
    Return,
    Try
}

public class TSScope
{
    public ScopeType Type { get; set; }

    public Dictionary<string, RuntimeObject> Variables { get; set; } = new();
}

using Cangjie.Core.Runtime;

namespace TypeSharp;

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

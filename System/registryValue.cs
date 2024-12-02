using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class registryValue
{
    private Json Target { get; }

    public registryValue()
    {
        Target = Json.NewObject();
    }

    public registryValue(Json target)
    {
        Target = target;
    }

    public static implicit operator registryValue(Json target)
    {
        return new registryValue(target);
    }

    public string value
    {
        get => Target.Read("value", string.Empty);
        set => Target.Set("value", value);
    }

    public string type
    {
        get => Target.Read("type", string.Empty);
        set => Target.Set("type", value);
    }
}

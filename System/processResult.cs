using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

public class processResult
{
    private Json Target { get; }

    public processResult()
    {
        Target = Json.NewObject();
    }

    public processResult(Json target)
    {
        Target = target;
    }

    public static implicit operator processResult(Json target)
    {
        return new processResult(target);
    }

    public int exitCode
    {
        get => Target.Read("exitCode", 0);
        set => Target.Set("exitCode", value);
    }

    public string output
    {
        get => Target.Read("output", "");
        set => Target.Set("output", value);
    }

    public string error
    {
        get => Target.Read("error", "");
        set => Target.Set("error", value);
    }
}

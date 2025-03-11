using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class processConfig
{
    private Json Target { get; }

    public processConfig()
    {
        Target = Json.NewObject();
    }

    public processConfig(Json target)
    {
        Target = target;
    }

    public static implicit operator processConfig(Json target)
    {
        return new processConfig(target);
    }

    public string filePath
    {
        get => Target.Read("filePath", "");
        set => Target.Set("filePath", value);
    }

    public Json arguments
    {
        get => Target.Get("arguments",Json.Null);
        set => Target.Set("arguments", value);
    }

    public string workingDirectory
    {
        get => Target.Read("workingDirectory", "");
        set => Target.Set("workingDirectory", value);
    }

    public bool useShellExecute
    {
        get => Target.Read("useShellExecute", false);
        set => Target.Set("useShellExecute", value);
    }

    public bool redirect
    {
        get => Target.Read("redirect", false); 
        set => Target.Set("redirect", value);
    }

    public bool createNoWindow
    {
        get => Target.Read("createNoWindow", true);
        set => Target.Set("createNoWindow", value);
    }

    public Json environment
    {
        get => Target.Get("environment", Json.Null);
        set => Target.Set("environment", value);
    }
}

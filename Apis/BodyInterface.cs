using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;

public class BodyInterface:IDisposable
{
    public BodyInterface()
    {

    }

    public BodyInterface(Json target)
    {
        Target = target;
    }

    public Json Target { get; set; } = Json.Null;

    public void Dispose()
    {
        Target = Json.Null;
    }

}

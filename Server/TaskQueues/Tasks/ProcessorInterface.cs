using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 处理者接口
/// </summary>
public class ProcessorInterface(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicit conversion from Json to ProcessorInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator ProcessorInterface(Json target) => new ProcessorInterface(target);

    /// <summary>
    /// Implicit conversion from ProcessorInterface to Json
    /// </summary>
    /// <param name="processor"></param>
    public static implicit operator Json(ProcessorInterface processor) => processor.Target;

    /// <summary>
    /// 处理者类型
    /// </summary>
    public ProcessorTypes Type
    {
        get=> Target.Read("Type", string.Empty) switch 
        {
            "Plugin" => ProcessorTypes.Plugin,
            "Script" => ProcessorTypes.Script,
            _ => ProcessorTypes.Unknown
        };
        set => Target.Set("Type", value switch
        {
            ProcessorTypes.Plugin => "Plugin",
            ProcessorTypes.Script => "Script",
            _ => "Unknown"
        });
    }

    /// <summary>
    /// 处理者名称
    /// </summary>
    public string Name
    {
        get=> Target.Read("Name", string.Empty);
        set=> Target.Set("Name", value);
    }
}

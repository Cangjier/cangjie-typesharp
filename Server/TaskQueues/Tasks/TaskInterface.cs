using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 任务封装
/// </summary>
public class TaskInterface(Json target):IDisposable
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target { get; } = target;

    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid id
    {
        get => Target.Read("id", Guid.Empty);
        set => Target.Set("id", value);
    }

    /// <summary>
    /// 输入数据
    /// </summary>
    public Json Input
    {
        get => Target.Get("Input", Json.Null);
        set => Target.Set("Input", value);
    }

    /// <summary>
    /// 输出数据
    /// </summary>
    public Json Output
    {
        get => Target.Get("Output", Json.Null);
        set => Target.Set("Output", value);
    }

    /// <summary>
    /// 任务接收者
    /// </summary>
    public Json Receiver => Target.GetOrCreateObject("Receiver");

    /// <summary>
    /// 任务处理者
    /// </summary>
    public ProcessorInterface Processor => Target.GetOrCreateObject("Processor");

    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskStatuses Status
    {
        get => Target.Read("Status", string.Empty) switch
        {
            "Pending" => TaskStatuses.Pending,
            "Running" => TaskStatuses.Running,
            "Completed" => TaskStatuses.Completed,
            "Failed" => TaskStatuses.Failed,
            _ => TaskStatuses.Pending
        };
        set => Target.Set("Status", value.ToString());
    }

    /// <summary>
    /// 跟踪信息
    /// </summary>
    public TraceInterface Trace => Target.GetOrCreateObject("Trace");

    /// <summary>
    /// Convert to TaskInterface
    /// </summary>
    /// <param name="task"></param>
    public static implicit operator Json(TaskInterface task) => task.Target;

    /// <summary>
    /// Convert to TaskInterface
    /// </summary>
    /// <param name="task"></param>
    public static implicit operator TaskInterface(Json task) => new (task);

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Target.Dispose();
    }

    /// <summary>
    /// 转换成字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Target.ToString();
    }
}
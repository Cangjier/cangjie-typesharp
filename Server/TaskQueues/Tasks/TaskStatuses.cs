namespace Cangjie.TypeSharp.Server.TaskQueues.Tasks;

/// <summary>
/// 任务状态
/// </summary>
public enum TaskStatuses
{
    /// <summary>
    /// 队列中
    /// </summary>
    Pending,
    /// <summary>
    /// 运行中
    /// </summary>
    Running,
    /// <summary>
    /// 已完成
    /// </summary>
    Completed,
    /// <summary>
    /// 已失败
    /// </summary>
    Failed,
}

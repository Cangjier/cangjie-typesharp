using TidyHPC.Schedulers;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 定时器
/// </summary>
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public class schedular
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    private DiscreteScheduler DiscreteScheduler = new();

    public string setTimeout(Action action, int delay)
    {
        var index = DiscreteScheduler.AddTask(TimeSpan.FromMilliseconds(delay), () =>
        {
            action();
            return Task.CompletedTask;
        });
        return $"{index.Key.Ticks},{index.ID}";
    }

    public void clearTimeout(string timeoutId)
    {

        var parts = timeoutId.Split(',');
        var key = new DateTime(long.Parse(parts[0]));
        var id = Guid.Parse(parts[1]);
        var index = new DiscreteScheduler.Index()
        {
            Key = key,
            ID = id
        };
        DiscreteScheduler.RemoveTask(index);
    }

}
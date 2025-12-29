using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Server.TaskQueues.Agents;

/// <summary>
/// CPU接口
/// </summary>
/// <param name="target"></param>
public class PerformanceInterface(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicitly convert Json to PerformanceInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator Json(PerformanceInterface target) => target.Target;

    /// <summary>
    /// Implicitly convert PerformanceInterface to Json
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator PerformanceInterface(Json target) => new PerformanceInterface(target);

    /// <summary>
    /// 使用率
    /// </summary>
    public double TotalProcessorTimePercent
    {
        get => Target.Read("TotalProcessorTimePercent", 0.0);
        set => Target.Set("TotalProcessorTimePercent", value);
    }

    /// <summary>
    /// 逻辑处理器数
    /// </summary>
    public int ProcessorCount
    {
        get => Target.Read("ProcessorCount", 0);
        set => Target.Set("ProcessorCount", value);
    }

    /// <summary>
    /// Memory Available Bytes
    /// </summary>
    public double MemoryAvailableBytes
    {
        get=> Target.Read("MemoryAvailableBytes", 0.0);
        set=> Target.Set("MemoryAvailableBytes", value);
    }

    /// <summary>
    /// Committed Bytes In Use Percent
    /// </summary>
    public double CommittedBytesInUsePercent
    {
        get=> Target.Read("CommittedBytesInUsePercent", 0.0);
        set=> Target.Set("CommittedBytesInUsePercent", value);
    }

    /// <summary>
    /// 逻辑处理器
    /// </summary>
    public CPUProcessorInterface[] Processors
    {
        get => Target.GetOrCreateArray("Processors").ToArray(x => new CPUProcessorInterface(x));
        set
        {
            var coresArray = Target.GetOrCreateArray("Processors");
            coresArray.Clear();
            foreach (var core in value)
            {
                coresArray.Add(core.Target);
            }
        }
    }

    /// <summary>
    /// Get Total Processor Time Percent
    /// </summary>
    /// <returns></returns>
    public static double GetTotalProcessorTimePercent()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PerformanceCounters.Processor.ProcessorTimePercent._Total?.NextValue() ?? 0;
        }
        return 0.0;
    }

    /// <summary>
    /// Get Processor Time Percent
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static double GetProcessorTimePercent(int index)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PerformanceCounters.Processor.ProcessorTimePercent.ByIndex(index)?.NextValue() ?? 0;
        }
        return 0.0;
    }

    /// <summary>
    /// Get Memory Available Bytes
    /// </summary>
    /// <returns></returns>
    public static double GetMemoryAvailableBytes()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PerformanceCounters.Memory.AvailableBytes?.NextValue() ?? 0;
        }
        return 0.0;
    }

    /// <summary>
    /// Get Committed Bytes In Use Percent
    /// </summary>
    /// <returns></returns>
    public static double GetCommittedBytesInUsePercent()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PerformanceCounters.Memory.CommittedBytesInUsePercent?.NextValue() ?? 0;
        }
        return 0.0;
    }

    /// <summary>
    /// Get Current Performance Interface
    /// </summary>
    /// <returns></returns>
    public static PerformanceInterface GetCurrent()
    {
        // 获取当前主机的CPU信息
        PerformanceInterface result = Json.NewObject();
        result.TotalProcessorTimePercent = GetTotalProcessorTimePercent();
        result.ProcessorCount = Environment.ProcessorCount;
        result.CommittedBytesInUsePercent = GetCommittedBytesInUsePercent();
        result.MemoryAvailableBytes = GetMemoryAvailableBytes();
        CPUProcessorInterface[] processors = new CPUProcessorInterface[result.ProcessorCount];
        for (int i = 0; i < result.ProcessorCount; i++)
        {
            CPUProcessorInterface processor = Json.NewObject();
            processor.ProcessorTimePercent = GetProcessorTimePercent(i);
            processors[i] = processor;
        }
        result.Processors = processors;
        return result;

    }
}

/// <summary>
/// CPU Core Interface
/// </summary>
/// <param name="target"></param>
public class CPUProcessorInterface(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Convert Json to CPUCoreInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator Json(CPUProcessorInterface target) => target.Target;

    /// <summary>
    /// Convert CPUCoreInterface to Json
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator CPUProcessorInterface(Json target) => new CPUProcessorInterface(target);

    /// <summary>
    /// 使用率
    /// </summary>
    public double ProcessorTimePercent
    {
        get => Target.Read("ProcessorTimePercent", 0.0);
        set => Target.Set("ProcessorTimePercent", value);
    }
}

/// <summary>
/// 性能计数器
/// </summary>
public static class PerformanceCounters
{
    static PerformanceCounters()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PerformanceCounterCache = new ConcurrentDictionary<string, PerformanceCounter?>();
        }
    }

    private static ConcurrentDictionary<string, PerformanceCounter?>? PerformanceCounterCache { get; }

    private static PerformanceCounter? GetPerformanceCounter(string category, string counter, string instance)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string key = $"{category}.{counter}.{instance}";
            if (PerformanceCounterCache!.TryGetValue(key, out PerformanceCounter? performanceCounter))
            {
                return performanceCounter;
            }
            performanceCounter = new PerformanceCounter(category, counter, instance);
            performanceCounter.NextValue();
            PerformanceCounterCache.TryAdd(key, performanceCounter);
            return performanceCounter;
        }
        return null;
    }

    /// <summary>
    /// 获取所有的性能计数器目录
    /// </summary>
    /// <returns></returns>
    public static PerformanceCounterCategory[]? GetCategories()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PerformanceCounterCategory.GetCategories();
        }
        return null;
    }

    /// <summary>
    /// 进程性能计数器
    /// </summary>
    public static class Processor
    {
        /// <summary>
        /// 处理器时间
        /// </summary>
        public static class ProcessorTimePercent
        {
            /// <summary>
            /// 总计
            /// </summary>
            public static PerformanceCounter? _Total => GetPerformanceCounter("Processor", "% Processor Time", "_Total");

            /// <summary>
            /// 获取指定索引的处理器
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static PerformanceCounter? ByIndex(int index) => GetPerformanceCounter("Processor", "% Processor Time", index.ToString());
        }
    }

    /// <summary>
    /// 内存性能计数器
    /// </summary>
    public static class Memory
    {
        /// <summary>
        /// 可用的字节数
        /// </summary>
        public static PerformanceCounter? AvailableBytes => GetPerformanceCounter("Memory", "Available Bytes", "");

        /// <summary>
        /// 已提交的字节数
        /// </summary>
        public static PerformanceCounter? CommittedBytes => GetPerformanceCounter("Memory", "Committed Bytes", "");

        /// <summary>
        /// 已提交的字节数的百分比
        /// </summary>
        public static PerformanceCounter? CommittedBytesInUsePercent => GetPerformanceCounter("Memory", "% Committed Bytes In Use", "");
    }

    /// <summary>
    /// 物理磁盘性能计数器
    /// </summary>
    public static class PhysicalDisk
    {

    }
}
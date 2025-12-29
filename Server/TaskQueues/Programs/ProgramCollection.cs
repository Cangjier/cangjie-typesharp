using Cangjie.Dawn;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Cangjie.TypeSharp.Server.TaskQueues.Programs;

/// <summary>
/// 程序集合
/// </summary>
public class ProgramCollection
{
    /// <summary>
    /// 程序集合
    /// </summary>
    public ConcurrentDictionary<string, IProgram> Programs { get; } = new();

    /// <summary>
    /// 添加程序
    /// </summary>
    /// <param name="id"></param>
    /// <param name="program"></param>
    public void Add(string id, IProgram program)
    {
        Programs[id] = program;
    }

    /// <summary>
    /// 移除程序
    /// </summary>
    /// <param name="id"></param>
    public void Remove(string id)
    {
        Programs.TryRemove(id, out _);
    }

    /// <summary>
    /// 尝试获取程序
    /// </summary>
    /// <param name="id"></param>
    /// <param name="program"></param>
    /// <returns></returns>
    public bool TryGet(string id, [NotNullWhen(true)]out IProgram? program)
    {
        return Programs.TryGetValue(id, out program);
    }

    /// <summary>
    /// 程序工厂
    /// </summary>
    public Func<string,string,IProgram>? CreateProgramByScriptContent { get; set; }

    /// <summary>
    /// 运行程序
    /// </summary>
    public Func<IProgram, string, string[],Task>? RunProgramByFilePathAndArgs { get; set; }

    /// <summary>
    /// 运行程序，带上下文
    /// </summary>
    public Func<IProgram,string,object,Task<IDisposable>>? RunProgramByFilePathAndContext { get; set; }

    /// <summary>
    /// 获取或创建程序
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IProgram GetOrCreate(string filePath)
    {
        var md5 = Util.GetFileMD5(filePath);
        if (TryGet(md5, out var program))
        {
            return program;
        }
        if(CreateProgramByScriptContent == null)
        {
            throw new InvalidOperationException("ProgramFactory is null");
        }
        program = CreateProgramByScriptContent(filePath,File.ReadAllText(filePath, Util.UTF8));
        Add(md5, program);
        return program;
    }
}

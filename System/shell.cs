using System.Diagnostics;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class shell
{
    /// <summary>
    /// 封装对象
    /// </summary>
    private Process Target { get; set; }

    private List<string> OutputLines { get; } = new();

    private List<string> ErrorLines { get; } = new();

    private Action<string>? OutputHandler { get; set; }

    public shell(Process process)
    {
        Target = process;
        Target.OutputDataReceived+= (sender, e) =>
        {
            if (e.Data != null)
            {
                OutputLines.Add(e.Data);
                OutputHandler?.Invoke(e.Data);
            }
        };
        Target.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                ErrorLines.Add(e.Data);
                OutputHandler?.Invoke(e.Data);
            }
        };
        Target.Start();
        Target.BeginOutputReadLine();
        Target.BeginErrorReadLine();
    }

    public static shell start(processConfig config)
    {
        Process process = new();
        process.StartInfo.FileName = config.filePath;
        if (config.workingDirectory != string.Empty) process.StartInfo.WorkingDirectory = config.workingDirectory;
        config.arguments.Foreach(item => process.StartInfo.ArgumentList.Add(item.AsString));
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        return new shell(process);
    }

    public static shell cmd(string workingDirectory, string commandLine)
    {
        return start(new processConfig
        {
            filePath = Util.GetShell(),
            workingDirectory = workingDirectory,
            arguments = new Json(Util.GetShellArguments(commandLine))
        });
    }

    public void kill()
    {
        Target.Kill();
    }

    public string read()
    {
        var result = OutputLines.Join("\r\n");
        OutputLines.Clear();
        return result;
    }

    public string[] readLines()
    {
        var result = OutputLines.ToArray();
        OutputLines.Clear();
        return result;
    }

    public async Task<string[]> readLinesWhen(Func<string, bool> predicate)
    {
        TaskCompletionSource<string[]> tcs = new();
        OutputHandler = (line) =>
        {
            if (predicate(line))
            {
                OutputHandler = null;
                tcs.SetResult(OutputLines.ToArray());
            }
        };
        return await tcs.Task;
    }

    public async Task when(Func<string, bool> predicate)
    {
        TaskCompletionSource<bool> tcs = new();
        OutputHandler = (line) =>
        {
            if (predicate(line))
            {
                OutputHandler = null;
                tcs.SetResult(true);
            }
        };
        await tcs.Task;
    }

    public void writeLine(string value)
    {
        Target.StandardInput.WriteLine(value);
    }

    public void write(string value)
    {
        Target.StandardInput.Write(value);
    }
}

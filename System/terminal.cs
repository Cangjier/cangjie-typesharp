using System.Collections.Concurrent;
using System.Text;
using TidyHPC.LiteJson;
using TidyHPC.Queues;
using TidyHPC.Terminal;

namespace Cangjie.TypeSharp.System;
/// <summary>
/// 终端管理
/// </summary>
public class terminal
{
    private static ConcurrentDictionary<Guid, terminalWrapper> Terminals { get; } = [];

    public static async Task<terminalWrapper> createAsync(terminalOptions options)
    {
        var terminal = new terminalWrapper(ITerminal.CreateTerminal());
        await terminal.start(options);
        Terminals[terminal.Target.ID] = terminal;
        return terminal;
    }

    public static void close(Guid id)
    {
        if (Terminals.TryRemove(id, out var terminal))
        {
            terminal.Target.Dispose();
        }
    }

    public static terminalWrapper get(Guid id)
    {
        if (Terminals.TryGetValue(id, out var terminal))
        {
            return terminal;
        }
        throw new Exception($"Terminal with ID {id} not found");
    }

    public static Guid[] list()
    {
        return Terminals.Keys.ToArray();
    }
}

public class terminalOptions(Json target)
{
    private Json Target = target;

    public static implicit operator terminalOptions(Json target) => new(target);

    public string? shell
    {
        get => Target.Read(nameof(shell), null!);
        set => Target[nameof(shell)] = value;
    }

    public string? workingDirectory
    {
        get => Target.Read(nameof(workingDirectory), null!);
        set => Target[nameof(workingDirectory)] = value;
    }

    public Json environmentVariables
    {
        get => Target.Get(nameof(environmentVariables), Json.Null);
        set => Target[nameof(environmentVariables)] = value;
    }

    public int columns
    {
        get => Target.Read(nameof(columns), 80);
        set => Target[nameof(columns)] = value;
    }

    public int rows
    {
        get => Target.Read(nameof(rows), 24);
        set => Target[nameof(rows)] = value;
    }
}

public class terminalWrapper
{
    public terminalWrapper(ITerminal target)
    {
        Target = target;
        target.OutputReceived += (bytes, length) =>
        {
            OutputBuffer.Write(bytes.AsSpan(0, length));
            OnData?.Invoke(bytes, length);
        };
    }

    private static UTF8Encoding utf8 = new();

    public ITerminal Target { get; }

    private Action<byte[], int>? OnData;

    private CircularBuffer OutputBuffer { get; } = new(4096);

    public void onOutput(Action<byte[], int> callback)
    {
        OnData = callback;
    }

    public async Task writeAsync(Json data)
    {
        if (data.IsString)
        {
            var bytes = utf8.GetBytes(data.AsString);
            await Target.WriteInputAsync(bytes, bytes.Length);
        }
        else if (data.Is<byte[]>())
        {
            var bytes = data.As<byte[]>();
            await Target.WriteInputAsync(bytes, bytes.Length);
        }
        else
        {
            throw new Exception($"Unsupported data type {data.Node?.GetType().Name} for terminal writeAsync");
        }
    }

    public async Task start(terminalOptions options)
    {
        var translatedOptions = new TerminalOptions();
        if (options.shell != null)
        {
            translatedOptions.Shell = options.shell;
        }
        if (options.workingDirectory != null)
        {
            translatedOptions.WorkingDirectory = options.workingDirectory;
        }
        if (options.environmentVariables.IsObject)
        {
            foreach (var pair in options.environmentVariables.GetObjectEnumerable())
            {
                translatedOptions.EnvironmentVariables[pair.Key] = pair.Value.AsString;
            }
        }
        translatedOptions.Columns = options.columns;
        translatedOptions.Rows = options.rows;
        await Target.StartAsync(translatedOptions);
    }

    public byte[] getOutput()
    {
        return OutputBuffer.ToArray();
    }
}


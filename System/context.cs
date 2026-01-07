using TidyHPC.LiteJson;
using TidyHPC.Loggers;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 上下文
/// </summary>
#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
public class Context : IDisposable
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
{
    public Context()
    {
        _apis = new(this);
        _console = new(this);
        _axios = new(this);
        _Logger = new LoggerFile();
        _context = Json.NewObject();
    }

    public Context(Context reference)
    {
        this.reference = reference;
    }

    public Context(LoggerFile logger)
    {
        _Logger = logger;
        _IsLoggerFileNeedDispose = false;
        _apis = new(this);
        _console = new(this);
        _axios = new(this);
        _context = Json.NewObject();
    }

    private Json _context = Json.Null;

    private Context? reference = null;

    public Json context => reference == null ? _context : reference.context;

    public Json getContext()
    {
        return context;
    }

    private Apis? _apis = null;

    public Apis apis => reference == null ? (_apis ?? throw new NullReferenceException(nameof(_apis))) : reference.apis;

    private Consoles? _console = null;

    public Consoles console => reference == null ? (_console ?? throw new NullReferenceException(nameof(_console))) : reference.console;

    private Axios? _axios = null;

    public Axios axios => reference == null ? (_axios ?? throw new NullReferenceException(nameof(_axios))) : reference.axios;

    private LoggerFile? _Logger = null;

    private bool _IsLoggerFileNeedDispose = true;

    public LoggerFile Logger
    {
        get => reference == null ? (_Logger ?? throw new NullReferenceException(nameof(_Logger))) : reference.Logger;
        set
        {
            if (reference == null)
            {
                _Logger = value;
            }
            else
            {
                reference.Logger = value;
            }
        }
    }

    private string[]? _args = null;

    /// <summary>
    /// 参数
    /// </summary>
    public string[] args
    {
        get => reference == null ? _args ?? throw new NullReferenceException(nameof(_args)) : reference.args;
        set
        {
            if (reference == null)
            {
                _args = value;
            }
            else
            {
                reference.args = value;
            }
        }
    }

    private Json _manifest = Json.Null;

    public Json manifest
    {
        get => reference == null ? _manifest : reference.manifest;
        set
        {
            if (reference == null)
            {
                _manifest = value;
            }
            else
            {
                reference.manifest = value;
            }
        }
    }

    public string script_path = "";

    public void setContext(Json context)
    {
        if (reference == null)
        {
            _context = context;
        }
        else
        {
            reference.setContext(context);
        }
    }

    public string locate(string path)
    {
        path = path.Trim(' ', '/', '\\');
        string?[] baseDirectories = [
            Path.GetDirectoryName(script_path),
            Environment.CurrentDirectory,
            Path.GetDirectoryName(Environment.ProcessPath),
        ];
        foreach (var baseDirectory in baseDirectories)
        {
            if (baseDirectory == null || Directory.Exists(baseDirectory) == false)
            {
                continue;
            }
            var result = staticContext.locate(baseDirectory, path);
            if (result != "")
            {
                return result;
            }
        }
        return "";
    }

    public Json eval(string script)
    {
        using var context = new Context(this);
        return TSScriptEngine.Run(script, context);
    }

    public async Task<Json> evalAsync(string script)
    {
        using var context = new Context(this);
        return await TSScriptEngine.RunAsync(script_path, script, context);
    }

    public void setLoggerPath(string path)
    {
        Logger.FilePath = path;
    }

    public string getLoggerPath()
    {
        return Logger.FilePath;
    }

    public void Dispose()
    {
        _args = null;
        script_path = null!;
        _apis?.Dispose();
        _apis = null!;
        if (_IsLoggerFileNeedDispose)
        {
            _Logger?.Dispose();
        }
        _Logger = null!;
        _console?.Dispose();
        _console = null!;
        _axios?.Dispose();
        _axios = null!;
        _context = Json.Null;
        reference = null;
    }
}

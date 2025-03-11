using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class program:IDisposable
{
    public program(string filePath)
    {
        _program = new(filePath, File.ReadAllText(filePath, Util.UTF8));
        _filePath = filePath;
        _context = null;
    }

    public program(string filePath, string code)
    {
        _program = new(filePath, code);
        _filePath = filePath;
        _context = null;
    }

    public program(string filePath,string code,Context context)
    {
        _program = new(filePath, code, context);
        _filePath = filePath;
        _context = context;
    }

    private TSProgram _program;

    private string _filePath;

    private Context? _context;

    public void Dispose()
    {
        _program.Dispose();
    }

    public async Task runAsync(string[] args)
    {
        try
        {
            using var context = _context ?? new Context();
            context.script_path = _filePath;
            context.args = args;
            await _program.RunAsync(context);
        }
        catch(Exception e)
        {
            throw new Exception(TSProgram.GetExceptionMessage(e));
        }
    }

    public async Task runAsync(string[] args,Json context)
    {
        try
        {
            using var _context = this._context ?? new Context();
            _context.setContext(context);
            _context.script_path = _filePath;
            _context.args = args;
            await _program.RunAsync(_context);
        }
        catch (Exception e)
        {
            throw new Exception(TSProgram.GetExceptionMessage(e));
        }
    }

    public static program load(string filePath) => new(filePath);

    public static program loadFile(string filePath, Json context)
    {
        Context _context = new ();
        _context.setContext(context);
        return new(filePath, File.ReadAllText(filePath, Util.UTF8), _context);
    }

    public static program load(string filePath, string code) => new(filePath, code);
}

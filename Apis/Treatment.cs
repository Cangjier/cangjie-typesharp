using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using Cangjie.TypeSharp;
using Cangjie.TypeSharp.System;

namespace Cangjie.TypeSharp.Cli.Apis;
public class Treatment:IDisposable
{
    public Treatment(Json parameters,Context context)
    {
        Parameters = parameters;
        ScriptContext = context;
        context.setContext(parameters);
    }

    public Context ScriptContext { get; private set; }

    public Json Parameters { get; set; } = Json.Null;

    public void RunCommands(Json commands)
    {
        commands.EachAll((value) =>
        {
            if (value.IsString)
            {
                var result = EvalString(value.AsString);
                Parameters.Set("ans", result);
                return true;
            }
            else
            {
                return value;
            }
        });
    }

    public Json EvalString(string script)
    {
        var task = TSScriptEngine.RunAsync(ScriptContext.script_path,script,ScriptContext);
        task.Wait();
        return task.Result;
    }

    public void Process(Json self, string[] skipKeys)
    {
        self.EachAll((path,subValue) =>
        {
            if (skipKeys.Contains(path.First.ToString())) return subValue;
            if (subValue.IsString)
            {
                if (subValue.AsString.StartsWith('$'))
                {
                    var result = EvalString(subValue.AsString[1..]);
                    return result;
                }
            }
            return subValue;
        });
    }

    public void InitialParameters(string? startupPath)
    {
        Parameters.Set("StartupPath", startupPath ?? "");
        Parameters.EachAll((subValue) =>
        {
            if (subValue.IsString)
            {
                if (subValue.AsString.StartsWith('$'))
                {
                    var result = EvalString(subValue.AsString[1..]);
                    return result;
                }
            }
            return subValue;
        });
    }

    public void CoverParametersBy(Json parameters)
    {
        if (parameters.IsObject)
        {
            parameters.GetObjectEnumerable().Foreach(pair =>
            {
                Parameters.Set(pair.Key, pair.Value.Clone());
            });
        }
    }

    public void Dispose()
    {
        ScriptContext = null!;
        Parameters = Json.Null;
    }
}

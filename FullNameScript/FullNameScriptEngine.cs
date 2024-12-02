using Cangjie.Core.Syntax.Templates;
using Cangjie.Owners;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Units;
using Cangjie.Dawn.Text.Units.Json;
using Cangjie.Dawn.Text.Units.String;

namespace Cangjie.TypeSharp.CSharpTypeFullNameEngine;

public class FullNameScriptEngine
{
    static FullNameScriptEngine()
    {

    }

    public static Template<char> Template { get; } = InitialTemplate(new());

    public static Template<char> InitialTemplate(Template<char> template)
    {
        template.BranchTemplate.AddModifyItem(typeof(StringGuide.Branch), branch =>
        {
            ((StringGuide.Branch)branch).AddStringChar('\'');
        });
        template.SymbolTemplate.Ban('_');
        template.ReorganizationTemplate.Ban(JsonArray.Reorganization.Instance, Statement.Reorganization2.Instance, Statement.Reorganization3.Instance);
        return template;
    }

    public static void Run(string script,Action<TextContext> onContext)
    {
        Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
        onContext(textContext);
        owner.Release();
    }
}

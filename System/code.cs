using Cangjie.Core.Syntax.Templates;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Tokens;
using Cangjie.Dawn.Text.Tokens.Json;
using Cangjie.Dawn.Text.Tokens.Lamda;
using Cangjie.Dawn.Text.Tokens.String;
using Cangjie.Owners;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// Code相关功能
/// </summary>
public class code
{
    private static Template<char> Template { get; } = InitialTemplate(new());

    private static Template<char> InitialTemplate(Template<char> template)
    {
        template.BranchTemplate.AddModifyItem(typeof(StringGuide.Branch), branch =>
        {
            ((StringGuide.Branch)branch).AddStringChar('`');
            ((StringGuide.Branch)branch).AddStringChar('\'');
        });
        template.ReorganizationTemplate.Ban(JsonObject.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(JsonArray.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(Method.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(Lamda.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(Let.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(TypeDefine.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(Statement.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(Statement.Reorganization2.Instance);
        template.ReorganizationTemplate.Ban(Statement.Reorganization3.Instance);
        template.ReorganizationTemplate.Ban(WrapSymbol.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(LineAnnotation.Reorganization.Instance);
        template.ReorganizationTemplate.Ban(AreaAnnotation.Reorganization.Instance);
        template.SymbolTemplate.Ban('_');
        return template;
    }

    /// <summary>
    /// 语义分析代码
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static Json analyse(string script)
    {
        Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
        var result = textContext.Root.ToList();
        owner.Release();
        return new Json(result);
    }
}

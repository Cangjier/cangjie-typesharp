using Cangjie.Core.Extensions;
using Cangjie.Core.Syntax;
using Cangjie.Core.Syntax.Templates;
using Cangjie.Dawn.Text;
using Cangjie.Dawn.Text.Units;
using Cangjie.Dawn.Text.Units.Interface;
using Cangjie.Owners;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// TypeScript Code相关功能
/// </summary>
public class typescript
{
    private static Template<char> Template { get; } = InitialTemplate(new());

    private static Template<char> InitialTemplate(Template<char> template)
    {
        return template;
    }

    /// <summary>
    /// 语义分析代码
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static Json getAllInterfaces(string script)
    {
        Json result = Json.NewArray();
        Owner owner = new();
        TextDocument document = new(owner, script);
        TextContext textContext = new(owner, Template);
        textContext.Process(document);
        var root = textContext.Root;
        void getAllProperties(List<Base<char>> units, Json properties, Json references)
        {
            bool isKey = true;
            List<Base<char>> keyUnits = [];
            List<Base<char>> valueUnits = [];
            bool isOptional = false;
            void getReferences(List<Base<char>> units, Json references)
            {
                foreach (var unit in units)
                {
                    if (unit is Common common && !common.Is(["string", "number", "boolean", "any", "void", "null", "undefined"]))
                    {
                        var reference = common.TempToString();
                        if (references.Contains(reference) == false)
                        {
                            references.Add(reference);
                        }
                    }
                    else if (unit is Bracket objectBracket && objectBracket.Is("{", "}"))
                    {
                        getAllProperties(objectBracket.Data, Json.Null, references);
                    }
                }
            }
            void completeProperty()
            {
                var keyStart = keyUnits.First().SourceRange.Start?.Index ?? throw new Exception("keyStart is null");
                var keyEnd = keyUnits.Last().SourceRange.End?.Index ?? throw new Exception("keyEnd is null");
                var valueStart = valueUnits.First().SourceRange.Start?.Index ?? throw new Exception("valueStart is null");
                var valueEnd = valueUnits.Last().SourceRange.End?.Index ?? throw new Exception("valueEnd is null");
                var key = document.GetRange(keyStart, keyEnd);
                var value = document.GetRange(valueStart, valueEnd);
                var valueTrim = value.Trim();
                bool containsArray = valueUnits.Contains(unit => unit is Bracket arrayBracket && arrayBracket.Is("[", "]"));
                bool containsObject = valueUnits.Contains(unit => unit is Bracket objectBracket && objectBracket.Is("{", "}"));
                bool isOneType = valueUnits.Where(unit => unit is Common).Count() == 1;
                getReferences(valueUnits, references);
                if (properties.IsObject)
                {
                    properties[key] = Json.NewObject().Set("isOptional", isOptional).Set("value", value);
                }
                isKey = true;
                keyUnits.Clear();
                valueUnits.Clear();
                isOptional = false;
            }
            void onUnit(Base<char> unit)
            {
                if (unit is LineAnnotation || unit is AreaAnnotation || unit is WrapSymbol)
                {
                    return;
                }
                if (isKey)
                {
                    if (unit is Symbol symbol && symbol.Is(":", "?:"))
                    {
                        isKey = false;
                        isOptional = symbol.Is("?:");
                    }
                    else
                    {
                        keyUnits.Add(unit);
                    }
                }
                else
                {
                    if (unit is Symbol symbol && symbol.Is(",", ";"))
                    {
                        completeProperty();
                    }
                    else
                    {
                        valueUnits.Add(unit);
                    }
                }
            }
            foreach (var unit in units)
            {
                onUnit(unit);
            }
            if (keyUnits.Count > 0 && valueUnits.Count > 0)
            {
                completeProperty();
            }
        }
        void onInterface(Base<char> unit)
        {
            if (unit is Interface @interface)
            {
                var interfaceObject = Json.NewObject();
                interfaceObject["name"] = @interface.InterfaceName;
                interfaceObject["extends"] = @interface.ExtendsInterfaceName;
                interfaceObject["isExport"] = @interface.IsExport;
                interfaceObject["raw"] = document.GetRange(@interface.SourceRange.Start!.Value.Index, @interface.SourceRange.End!.Value.Index);
                var properties = interfaceObject.GetOrCreateObject("properties");
                var references = interfaceObject.GetOrCreateArray("references");
                getAllProperties(@interface.Body.Data, properties, references);
                result.Add(interfaceObject);
            }
            else if (unit.Data.Count > 0)
            {
                foreach (var item in unit.Data)
                {
                    onInterface(item);
                }
            }
        }
        onInterface(root);
        owner.Release();
        return result;
    }

}


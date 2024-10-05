using Cangjie.Core.Exceptions;
using Cangjie.Core.Syntax;
using Cangjie.Dawn.Text.Units;
using Cangjie.TypeSharp.CSharpTypeFullNameEngine;
using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.FullNameScript;
public class FullName
{
    public string NameSpace { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public string AssemblyFullName { get; set; } = string.Empty;

    public bool IsArray { get; set; } = false;

    public int ArrayRank { get; set; } = 0;

    public bool IsGeneric { get; set; } = false;

    public int GenericArity { get; set; } = 0;

    public FullName[] GenericTypes { get; set; } = [];

    public Dictionary<string, string> Attributes { get; } = [];

    private enum Flag
    {
        Namespace,
        Generic,
        Array,
        Assembly,
        KeyValueKey,
        KeyValueValue
    }

    public static FullName Parse(IEnumerable<Base<char>> items)
    {
        Flag flag = Flag.Namespace;// 0:namespace, 1:generic, 2:array, 3:assembly, 4:key-value/key , 5:key-value/value
        List<Base<char>> namespaceWithTypeNameBases = [];
        List<Base<char>> assemblyFullNameBases = [];
        List<FullName> genericTypes = [];
        List<Base<char>[]> keys = [];
        List<Base<char>[]> values = [];
        List<Base<char>> keyTemp = [];
        List<Base<char>> valueTemp = [];
        int genericArity = 0;
        bool isArray = false;
        int arrayRank = 0;
        int count = items.Count();
        int index = -1;
        foreach (var item in items)
        {
            index++;
            if (flag == Flag.Namespace)
            {
                if (item is Symbol symbol)
                {
                    if (symbol.Is("`"))
                    {
                        flag = Flag.Generic;
                    }
                    else if (symbol.Is(","))
                    {
                        flag = Flag.Assembly;
                    }
                    else
                    {
                        namespaceWithTypeNameBases.Add(item);
                    }
                }
                else if(item is Bracket bracket && bracket.StartBracketChar == '[')
                {
                    flag = Flag.Array;
                    isArray = true;
                    arrayRank++;
                }
                else
                {
                    namespaceWithTypeNameBases.Add(item);
                }
            }
            else if (flag == Flag.Generic)
            {
                if(item is Common common)
                {
                    genericArity = int.Parse(common.TempToString());
                }
                else if (item is Bracket bracket && bracket.StartBracketChar == '[')
                {
                    if (genericTypes.Count == 0)
                    {
                        foreach (var genericItem in bracket.Data)
                        {
                            if (genericItem is Bracket genericFullName)
                            {
                                genericTypes.Add(Parse(genericFullName.Data));
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        isArray = true;
                        arrayRank++;
                        flag = Flag.Array;
                    }
                }
                else if (item is Symbol symbol && symbol.Is(","))
                {
                    flag = Flag.Assembly;
                }
                else
                {
                    throw new SyntaxException<char>(item.SourceRange);
                }
            }
            else if (flag == Flag.Assembly)
            {
                if (item is Symbol symbol && symbol.Is(","))
                {
                    flag = Flag.KeyValueKey;
                }
                else
                {
                    assemblyFullNameBases.Add(item);
                }
            }
            else if (flag == Flag.KeyValueKey)
            {
                if (item is Symbol symbol && symbol.Is("="))
                {
                    flag = Flag.KeyValueValue;
                    keys.Add(keyTemp.ToArray());
                    keyTemp.Clear();
                }
                else
                {
                    keyTemp.Add(item);
                }
            }
            else if (flag == Flag.KeyValueValue)
            {
                if (item is Symbol symbol && symbol.Is(","))
                {
                    flag = Flag.KeyValueKey;
                    values.Add(valueTemp.ToArray());
                    valueTemp.Clear();
                }
                else if(index == count - 1)
                {
                    valueTemp.Add(item);
                    flag = Flag.KeyValueKey;
                    values.Add(valueTemp.ToArray());
                    valueTemp.Clear();
                }
                else
                {
                    valueTemp.Add(item);
                }
            }
            else if (flag == Flag.Array)
            {
                if (item is Symbol symbol && symbol.Is(","))
                {
                    flag = Flag.Assembly;
                }
                else if (item is Bracket bracket && bracket.StartBracketChar == '[')
                {
                    arrayRank++;
                }
                else throw new SyntaxException<char>(item.SourceRange);
            }
        }
        FullName fullName = new();
        if (namespaceWithTypeNameBases.Count > 2)
        {
            fullName.NameSpace = namespaceWithTypeNameBases.SkipLast(2).Join("", item =>
            {
                if (item is Block<char> block) return block.TempToString();
                else throw new SyntaxException<char>(item.SourceRange);
            });
            fullName.TypeName = namespaceWithTypeNameBases.Last() is Block<char> block ? 
                block.TempToString() : 
                throw new SyntaxException<char>(namespaceWithTypeNameBases.Last().SourceRange);
        }
        else
        {
            fullName.NameSpace = string.Empty;
            fullName.TypeName = namespaceWithTypeNameBases.Last() is Block<char> block ?
                block.TempToString() :
                throw new SyntaxException<char>(namespaceWithTypeNameBases.Last().SourceRange);
        }
        fullName.AssemblyFullName = assemblyFullNameBases.Join("", item =>
        {
            if (item is Block<char> block) return block.TempToString();
            else throw new SyntaxException<char>(item.SourceRange);
        });
        fullName.IsGeneric = genericTypes.Count > 0;
        fullName.GenericTypes = genericTypes.ToArray();
        fullName.GenericArity = genericArity;
        fullName.Attributes.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i].Join("", item =>
            {
                if (item is Block<char> block) return block.TempToString();
                else throw new SyntaxException<char>(item.SourceRange);
            });
            string value = values[i].Join("", item =>
            {
                if (item is Block<char> block) return block.TempToString();
                else throw new SyntaxException<char>(item.SourceRange);
            });
            fullName.Attributes.Add(key, value);
        }
        fullName.IsArray = isArray;
        fullName.ArrayRank = arrayRank;
        return fullName;
    }

    public static FullName Parse(string fullName)
    {
        FullName? result = null;
        FullNameScriptEngine.Run(fullName, context =>
        {
            result = Parse(context.Root.Data);
        });
        if (result == null) throw new NullReferenceException();
        return result;
    }

    public override string ToString()
    {
        List<string> tokens = [];
        if (NameSpace != string.Empty)
        {
            tokens.Add(NameSpace);
            tokens.Add(".");
        }
        tokens.Add(TypeName);
        if (IsGeneric)
        {
            tokens.Add("`");
            tokens.Add(GenericArity.ToString());
            tokens.Add("[");
            tokens.Add(GenericTypes.Join(",", item => $"[{item}]"));
            tokens.Add("]");
        }
        if (IsArray)
        {
            for(int i = 0; i < ArrayRank; i++)
            {
                tokens.Add("[]");
            }
        }
        if (AssemblyFullName.Length != 0)
        {
            tokens.Add(", ");
            tokens.Add(AssemblyFullName);
        }
        if (Attributes.Count > 0)
        {
            tokens.Add(", ");
            tokens.Add(Attributes.Select(item => $"{item.Key}={item.Value}").Join(", "));
        }
        return string.Join("", tokens);
    }
}

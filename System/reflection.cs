using System.Reflection;
using System.Text.RegularExpressions;
using Cangjie.TypeSharp.FullNameScript;

namespace Cangjie.TypeSharp.System;
#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
public static class reflection
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
{
    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type? getType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }

    public static Type[] getTypes(string regexString)
    {
        List<Type> types = [];
        Regex regex = new(regexString);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.FullName == null)
                {
                    continue;
                }
                if (regex.IsMatch(type.FullName))
                {
                    types.Add(type);
                }
            }
        }
        return types.ToArray();
    }

    public static bool isParams(ParameterInfo parameter)
    {
        return parameter.GetCustomAttribute<ParamArrayAttribute>() != null;
    }

    public static FullName parseFullName(string fullName) => FullName.Parse(fullName);
}

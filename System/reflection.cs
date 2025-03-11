using System.Reflection;
using System.Text.RegularExpressions;
using Cangjie.TypeSharp.FullNameScript;
using TidyHPC.LiteJson;

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
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }

    public static Type? getTypeByFullName(FullName fullName)
    {
        return getType($"{fullName.NameSpace}.{fullName.TypeName}");
    }

    public static Type[] getTypes(string regexString)
    {
        List<Type> types = [];
        Regex regex = new(regexString);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
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

    public static Type[] getEnumarables(Type type)
    {
        List<Type> types = [];
        foreach (var i in type.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                types.Add(i.GetGenericArguments()[0]);
            }
        }
        return types.ToArray();
    }

    public static bool isImplicitFromJson(Type type)
    {
        if (type == typeof(Json)) return false;
        return type.GetMethod("op_Implicit", [typeof(Json)]) != null;
    }

    public static AssemblyName[] getReferencedAssemblies(Assembly assembly,int deepth=2)
    {
        void _getReferencedAssemblies(Assembly assembly, int deepth, List<AssemblyName> assemblyNames)
        {
            if (deepth == 0)
            {
                return;
            }
            var references = assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                if (assemblyNames.Any(i => i.FullName == reference.FullName))
                {
                    continue;
                }
                assemblyNames.Add(reference);
                var refAssembly = Assembly.Load(reference);
                _getReferencedAssemblies(refAssembly, deepth - 1, assemblyNames);
            }
        }
        List<AssemblyName> assemblyNames = [];
        _getReferencedAssemblies(assembly, deepth, assemblyNames);
        return assemblyNames.ToArray();
    }

    public static void loadDependiencies(Assembly assembly,int depth)
    {
        var references = getReferencedAssemblies(assembly, depth);
        foreach (var reference in references)
        {
            Assembly.Load(reference);
        }
    }

    public static void loadDependiencies(int depth)
    {
        loadDependiencies(Assembly.GetCallingAssembly(), depth);
    }
}

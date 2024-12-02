using Microsoft.Win32;
using System.Runtime.InteropServices;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
/// <summary>
/// 注册表
/// </summary>
public class registry
{
    /// <summary>
    /// 获取键值
    /// </summary>
    /// <param name="registryPath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static registryValue? get(string registryPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var root = registryPath.Split('\\')[0];
            var path = registryPath.Substring(root.Length + 1);
            using var reg = RegistryKey.OpenBaseKey(root.ToUpper() switch
            {
                "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKEY_USERS" => RegistryHive.Users,
                "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
                _ => throw new ArgumentException("Invalid root key")
            }, RegistryView.Default);
            using var key = reg.OpenSubKey(path);
            if (key != null)
            {
                var value = key.GetValue(null);
                var type = key.GetValueKind(null);
                return new registryValue
                {
                    value = value?.ToString() ?? string.Empty,
                    type = type.ToString()
                };
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 设置键值
    /// </summary>
    /// <param name="registryPath"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void set(string registryPath, registryValue? value)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var root = registryPath.Split('\\')[0];
            var path = registryPath.Substring(root.Length + 1);
            using var reg = RegistryKey.OpenBaseKey(root.ToUpper() switch
            {
                "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKEY_USERS" => RegistryHive.Users,
                "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
                _ => throw new ArgumentException("Invalid root key")
            }, RegistryView.Default);
            using var key = reg.CreateSubKey(path);
            if (key != null && value!=null)
            {
                key.SetValue(null, value.type.ToLower() switch
                {
                    "string" => value.value,
                    "expandstring" => value.value,
                    "binary" => Convert.FromBase64String(value.value),
                    "dword" => int.Parse(value.value),
                    "multistring" => value.value.Split('\0'),
                    "qword" => long.Parse(value.value),
                    _ => throw new ArgumentException("Invalid value type")
                }, value.type.ToLower() switch
                {
                    "string" => RegistryValueKind.String,
                    "expandstring" => RegistryValueKind.ExpandString,
                    "binary" => RegistryValueKind.Binary,
                    "dword" => RegistryValueKind.DWord,
                    "multistring" => RegistryValueKind.MultiString,
                    "qword" => RegistryValueKind.QWord,
                    _ => throw new ArgumentException("Invalid value type")
                });
            }
        }
    }
}

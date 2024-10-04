using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Cangjie.TypeSharp;

/// <summary>
/// Process extensions
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Get command line
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static string GetCommandLine(this Process self)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string query = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {self.Id}";

            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["CommandLine"] != null)
                {
                    return obj["CommandLine"].ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string cmdlinePath = $"/proc/{self.Id}/cmdline";
            if (File.Exists(cmdlinePath))
            {
                return File.ReadAllText(cmdlinePath).Replace('\0', ' ');
            }
            return string.Empty;
        }
        else
        {
            return string.Empty;
        }
    }
}

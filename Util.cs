﻿using Cangjie.TypeSharp.System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp;
public class Util
{
    public static Encoding UTF8 { get; } = new UTF8Encoding(false);

    public static Json EvalString(string script,Context context)
    {
        return TSScriptEngine.Run(script, context);
    }

    public static string TryEvalString(string script,Context context)
    {
        if(script.StartsWith("$"))
        {
            return EvalString(script[1..], context).ToString();
        }
        return script;
    }

    public static string ComputeSHA256Hash(string rawData)
    {
        // 创建一个SHA256对象  
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // 将输入字符串转换为字节数组  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // 将字节数组转换为十六进制字符串  
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public static string ComputeSHA256Hash(byte[] rawData)
    {
        // 创建一个SHA256对象  
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // 将输入字符串转换为字节数组  
            byte[] bytes = sha256Hash.ComputeHash(rawData);

            // 将字节数组转换为十六进制字符串  
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public static int ComputeHash(string input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256Hash.ComputeHash(bytes);

            // 取哈希值的前4个字节转换为一个整数  
            int intHash = BitConverter.ToInt32(hash, 0);
            return intHash;
        }
    }

    public static string ComputeMD5Hash(string rawData)
    {
        // 创建一个MD5对象  
        using MD5 md5Hash = MD5.Create();
        // 将输入字符串转换为字节数组并计算哈希数据  
        byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        // 创建一个 Stringbuilder 来收集字节并创建字符串  
        StringBuilder builder = new StringBuilder();

        // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        // 返回十六进制字符串  
        return builder.ToString();
    }

    public static string ComputeMD5Hash(byte[] rawData)
    {
        // 创建一个MD5对象  
        using MD5 md5Hash = MD5.Create();
        // 将输入字符串转换为字节数组并计算哈希数据  
        byte[] bytes = md5Hash.ComputeHash(rawData);

        // 创建一个 Stringbuilder 来收集字节并创建字符串  
        StringBuilder builder = new StringBuilder();

        // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        // 返回十六进制字符串  
        return builder.ToString();
    }

    public static string ComputeMD5Hash(FileStream stream)
    {
        // 创建一个MD5对象  
        using (MD5 md5Hash = MD5.Create())
        {
            // 将输入字符串转换为字节数组并计算哈希数据  
            byte[] bytes = md5Hash.ComputeHash(stream);

            // 创建一个 Stringbuilder 来收集字节并创建字符串  
            StringBuilder builder = new StringBuilder();

            // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            // 返回十六进制字符串  
            return builder.ToString();
        }
    }

    public static string ComputeMD5HashByFilePath(string filePath)
    {
        using FileStream stream = new(filePath, FileMode.Open);
        return ComputeMD5Hash(stream);
    }

    public static string ComputeBase64(string rawData)
    {
        return Convert.ToBase64String(UTF8.GetBytes(rawData));
    }

    public static string ComputeBase64(byte[] rawData)
    {
        return Convert.ToBase64String(rawData);
    }

    public static string GetShell()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return "cmd.exe"; // Windows 下使用 cmd
        }
        else
        {
            return "/bin/bash"; // Linux/MacOS 下使用 bash
        }
    }

    public static string GetShellArguments(string command)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            string escapedCommand = command;
            return $"/c {escapedCommand}"; // Windows 下 cmd 需要使用 /c 来执行命令
        }
        else
        {
            // 处理命令行中的双引号情况
            string escapedCommand = command.Replace("\"", "\\\"");
            return $"-c \"{escapedCommand}\""; // Linux/MacOS 下 bash 使用 -c 参数
        }
    }

    public static string GetGitProxy()
    {
        return staticContext.cmd(Environment.CurrentDirectory, "git config --global http.proxy").output;
    }

    public static string GetSystemProxy()
    {
        if(WebRequest.DefaultWebProxy?.GetProxy(new Uri("http://www.example.com")) is Uri webProxy)
        {
            return webProxy.ToString();
        }
        return "";
    }

    public static string GetRawUrl(string url)
    {
        //将github地址转换为raw地址，例如：
        //https://github.com/Cangjier/type-sharp/blob/main/cli/create-react-component/main.ts
        //https://raw.githubusercontent.com/Cangjier/type-sharp/main/cli/create-react-component/main.ts
        if (url.StartsWith("https://github.com/") || url.Contains("/blob/"))
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var segments = path.Split('/');
            return $"https://raw.githubusercontent.com/{segments.Skip(1).Where(item => item != "blob").Join("/")}";
        }
        return url;
    }

    public static HttpClient HttpClient { get; } = new();

    public static async Task<string> HttpGetAsString(string url)
    {
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

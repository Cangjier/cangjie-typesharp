﻿using Cangjie.TypeSharp.System;

namespace Cangjie.TypeSharp.Cli;
public class GitRepository
{
    public string RepositoryUrl { get; set; } = "https://github.com/Cangjier/type-sharp.git";

    public string ApplicationName { get; set; } = ".tscl";

    private string _gitRepositoryDirectory = string.Empty;

    public string GitRepositoryDirectory
    {
        get
        {
            if (string.IsNullOrEmpty(_gitRepositoryDirectory))
            {
                _gitRepositoryDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ??
                    Environment.GetEnvironmentVariable("home") ??
                    Environment.GetEnvironmentVariable("HOME") ??
                    throw new NullReferenceException("No home varible"),
                    $"{ApplicationName}/repository");
            }
            return _gitRepositoryDirectory;
        }
        set
        {
            _gitRepositoryDirectory = value;
        }
    }

    private string CliDirectory => Path.Combine(GitRepositoryDirectory, "cli");

    public async Task Update()
    {
        if (!Directory.Exists(GitRepositoryDirectory))
        {
            Directory.CreateDirectory(GitRepositoryDirectory);
        }
        // 判断是否存在.git文件夹，如果不存在，则执行git clone
        var gitDirectory = Path.Combine(GitRepositoryDirectory, ".git");
        if (Directory.Exists(gitDirectory) == false)
        {
            await staticContext.cmdAsync(GitRepositoryDirectory, $"git clone --depth 1 {RepositoryUrl} .");
        }
        // 执行git pull
        await staticContext.cmdAsync(GitRepositoryDirectory, "git pull");
    }

    public IEnumerable<string> ListCli()
    {
        if (Directory.Exists(CliDirectory) == false) return [];
        return Directory.GetDirectories(CliDirectory).Select(item => Path.GetFileName(item));
    }

    public string? GetCliScriptPath(string scriptName)
    {
        var scriptDirectory = Path.Combine(CliDirectory, scriptName);
        if (Directory.Exists(scriptDirectory) == false) return null;
        var files = Directory.GetFiles(scriptDirectory, "*.ts");
        var mainFile = files.FirstOrDefault(item => Path.GetFileName(item).ToLower() == "main.ts" || Path.GetFileName(item).ToLower() == "index.ts");
        return mainFile;
    }
}

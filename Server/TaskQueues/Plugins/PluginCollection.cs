using System.Collections.Concurrent;
using System.IO.Compression;
using TidyHPC.Common;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Schedulers;
namespace Cangjie.TypeSharp.Server.TaskQueues.Plugins;

/// <summary>
/// 插件管理器
/// </summary>
public class PluginCollection
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public PluginCollection(TaskService taskService)
    {
        TaskService = taskService;
        PluginDirectory = Util.GetSpecialDirectory("Plugins");
        _ = Task.Run(DiscreteScheduler.StartAsync);
        ListenPluginsDirectoryChanged();
    }

    /// <summary>
    /// 任务服务
    /// </summary>
    public TaskService TaskService { get; }

    /// <summary>
    /// 插件字典, key: 插件名称, value: 插件封装
    /// </summary>
    public ConcurrentDictionary<string, PluginInterface> Plugins { get; } = new();

    private ConcurrentDictionary<string, string> PluginPaths { get; } = new();
    
    /// <summary>
    /// 当重新加载插件时
    /// </summary>
    public Action? OnLoadedPlugins { get; set; }

    private bool _Enable = false;

    /// <summary>
    /// 是否启用插件
    /// </summary>
    public bool Enable
    {
        get=>_Enable;
        set
        {
            if (_Enable != value)
            {
                _Enable = value;
                if (value)
                {
                    ReloadPlugins();
                }
                else
                {
                    Plugins.Clear();
                    PluginPaths.Clear();
                }
            }
           
        }
    }

    private DiscreteScheduler DiscreteScheduler { get; } = new();

    private string _PluginDirectory = string.Empty;

    /// <summary>
    /// 本地插件目录
    /// </summary>
    public string PluginDirectory
    {
        get => _PluginDirectory;
        set
        {
            if (_PluginDirectory != value)
            {
                _PluginDirectory = value;
                if (FileSystemWatcher != null)
                {
                    FileSystemWatcher.Dispose();
                }
                ListenPluginsDirectoryChanged();
                ReloadPlugins();
            }
        }
    }

    private FileSystemWatcher? FileSystemWatcher { get; set; }

    private void TryAddPlugin( PluginInterface pluginWrap, string packagePath)
    {
        string pluginName = pluginWrap.Name.ToLower();
        if (Plugins.TryGetValue(pluginName, out var oldPlugin))
        {
            if (pluginWrap.Priority > oldPlugin.Priority)
            {
                Plugins[pluginName] = pluginWrap;
                PluginPaths[pluginName] = packagePath;
                Logger.Info($"加载插件:{pluginWrap.Name}");
            }
        }
        else
        {
            Plugins[pluginName] = pluginWrap;
            PluginPaths[pluginName] = packagePath;
            Logger.Info($"加载插件:{pluginWrap.Name}");
        }
    }

    private void LoadPlugins()
    {
        Logger.Info("Start Loading Plugins");
        if (!Directory.Exists(PluginDirectory))
        {
            Directory.CreateDirectory(PluginDirectory);
        }
        var packagePaths = Directory.GetFiles(PluginDirectory, "package.json", SearchOption.AllDirectories);
        int index = 0;
        foreach (var packagePath in packagePaths)
        {
            Logger.Info($"{index++}/{packagePaths.Length} {packagePath}");
            try
            {
                var plugin = Json.Load(packagePath);
                if (plugin.IsObject)
                {
                    var pluginWrap = new PluginInterface(plugin);
                    if (pluginWrap.IsEnable)
                    {
                        TryAddPlugin(pluginWrap, packagePath);
                    }
                }
                else if (plugin.IsArray)
                {
                    plugin.GetArrayEnumerable().Foreach(item =>
                    {
                        var pluginWrap = new PluginInterface(item);
                        if (pluginWrap.IsEnable)
                        {
                            TryAddPlugin(pluginWrap, packagePath);
                        }
                    });
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
        }
        OnLoadedPlugins?.Invoke();
    }

    /// <summary>
    /// 重新加载
    /// </summary>
    public void ReloadPlugins()
    {
        Plugins.Clear();
        PluginPaths.Clear();
        if (Enable)
        {
            LoadPlugins();
        }
    }

    /// <summary>
    /// 获取插件路径
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public string GetPluginPath(string pluginName)
    {
        return PluginPaths[pluginName.ToLower()];
    }

    /// <summary>
    /// 是否包含插件
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public bool ContainsPlugin(string pluginName)
    {
        return Plugins.ContainsKey(pluginName.ToLower());
    }

    /// <summary>
    /// 尝试获取插件
    /// </summary>
    /// <param name="pluginName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryGetPlugin(string pluginName,out PluginInterface result)
    {
        return Plugins.TryGetValue(pluginName.ToLower(), out result);
    }

    private DiscreteScheduler.Index NotifyScheduler { get; set; }

    private SemaphoreSlim NodifySemaphore { get; } = new(1);

    private SemaphoreSlim ChangeSemaphore { get; } = new(1);

    private async Task TryNotifyScheduler()
    {
        if (Enable == false)
        {
            return;
        }
        await NodifySemaphore.WaitAsync();
        if (NotifyScheduler.IsValid)
        {
            DiscreteScheduler.RemoveTask(NotifyScheduler);
        }
        NotifyScheduler = DiscreteScheduler.AddTask(TimeSpan.FromSeconds(4), async () =>
        {
            await ChangeSemaphore.WaitAsync();
            if (Enable)
            {
                Logger.Info("Reload Plugins");
                ReloadPlugins();
            }
            ChangeSemaphore.Release();
        });
        NodifySemaphore.Release();
    }

    private void ListenPluginsDirectoryChanged()
    {
        if (Directory.Exists(PluginDirectory))
        {
            FileSystemWatcher = new FileSystemWatcher(PluginDirectory);
            FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            FileSystemWatcher.IncludeSubdirectories = true;
            FileSystemWatcher.Changed += (sender, args) =>
            {
                if (args.FullPath.EndsWith("package.json") == false)
                {
                    return;
                }
                _ = TryNotifyScheduler();
            };
            FileSystemWatcher.EnableRaisingEvents = true;
            Logger.Info($"Listening PluginDirectory, {PluginDirectory}");
        }
        else
        {
            Logger.Info($"PluginDirectory not exists, {PluginDirectory}");
            FileSystemWatcher = null;
        }
    }

    /// <summary>
    /// 获取所有插件
    /// </summary>
    /// <param name="onItem"></param>
    public void GetPlugins(Action<PluginInterface> onItem)
    {
        foreach (var plugin in Plugins)
        {
            onItem(plugin.Value);
        }
    }
    
    /// <summary>
    /// 根据条件过滤插件
    /// </summary>
    /// <param name="onPredicate"></param>
    /// <returns></returns>
    public IEnumerable<PluginInterface> FilterPlugins(Func<PluginInterface,bool> onPredicate)
    {
        foreach (var plugin in Plugins)
        {
            if (onPredicate(plugin.Value))
            {
                yield return plugin.Value;
            }
        }
    }

    /// <summary>
    /// 运行TypeSharp服务
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<IDisposable> RunTypeSharpService(object context)
    {
        ToDispose toDispose = new();
        var tsPlugin = Plugins.Values.Where(item => item.Type.ToLower() == "typesharp-server");
        List<Task<IDisposable>> tasks = [];
        foreach (var plugin in tsPlugin)
        {
            tasks.Add(plugin.RunTypeSharpServer(TaskService,context));
        }
        var disposes = await Task.WhenAll(tasks);
        toDispose.Add(disposes);
        return toDispose;
    }
}
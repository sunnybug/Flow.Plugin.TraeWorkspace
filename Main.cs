using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Flow.Plugin.TraeWorkspace.RemoteMachinesHelper;
using Flow.Plugin.TraeWorkspace.TraeHelper;
using Flow.Plugin.TraeWorkspace.WorkspacesHelper;

namespace Flow.Plugin.TraeWorkspace
{
    /// <summary>
    /// 主插件类，实现Flow Launcher插件接口
    /// </summary>
    public class Main : IPlugin, IPluginI18n, ISettingProvider, IContextMenu
    {
        internal static PluginInitContext _context { get; private set; }
        private static Settings _settings;

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name => "Trae Workspaces";

        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description => "快速打开Trae最近工作区和远程SSH连接";

        private TraeInstance defaultInstance;
        private readonly TraeWorkspacesApi _workspacesApi = new();
        private readonly TraeRemoteMachinesApi _machinesApi = new();

        /// <summary>
        /// 处理查询请求
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>查询结果列表</returns>
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            var workspaces = new List<TraeWorkspaceItem>();

            // User defined extra workspaces
            if (defaultInstance != null)
            {
                var customWorkspaces = _settings.CustomWorkspaces.Select(uri =>
                    TraeWorkspacesApi.ParseVSCodeUri(uri, defaultInstance)).ToList();
                workspaces.AddRange(customWorkspaces);
            }

            // Search opened workspaces
            if (_settings.DiscoverWorkspaces)
            {
                try
                {
                    var discoveredWorkspaces = _workspacesApi.Workspaces;
                    workspaces.AddRange(discoveredWorkspaces);
                }
                catch (Exception ex)
                {
                    // 记录错误但继续执行
                    Console.WriteLine($"获取工作区失败: {ex.Message}");
                }
            }

            // Simple de-duplication
            var distinctWorkspaces = workspaces.Distinct().ToList();
            results.AddRange(distinctWorkspaces.Select(CreateWorkspaceResult));

            // Search opened remote machines
            if (_settings.DiscoverMachines)
            {
                try
                {
                    results.AddRange(GetResultFromOpenedRemoteMachines());
                }
                catch (Exception ex)
                {
                    // 记录错误但继续执行
                    Console.WriteLine($"获取远程机器失败: {ex.Message}");
                }
            }

            if (query.ActionKeyword == string.Empty ||
                (query.ActionKeyword != string.Empty && query.Search != string.Empty))
            {
                results = results.Where(r =>
                {
                    var matchResult = _context.API.FuzzySearch(query.Search, r.Title);
                    r.Score = matchResult.Score;
                    if (r.Score == 0 && !string.IsNullOrWhiteSpace(query.Search) &&
                        r.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
                    {
                        r.Score = 1;
                    }
                    return r.Score > 0;
                }).ToList();
            }

            return results;
        }

        private List<Result> GetResultFromOpenedRemoteMachines()
        {
            var results = new List<Result>();
            _machinesApi.Machines.ForEach(a =>
            {
                var title = $"SSH: {a.Host}";
                if (!string.IsNullOrEmpty(a.User) && !string.IsNullOrEmpty(a.HostName))
                    title += $" [{a.User}@{a.HostName}]";

                results.Add(new Result
                {
                    Title = title,
                    SubTitle = "远程SSH连接",
                    Icon = () => a.TraeInstance.RemoteIcon,
                    Action = c =>
                    {
                        try
                        {
                            var traeExecutable = a.TraeInstance.ExecutablePath;
                            _context.API.LogInfo($"{Name}", $"使用Trae可执行文件: {traeExecutable}");
                            var process = new ProcessStartInfo
                            {
                                FileName = traeExecutable,
                                UseShellExecute = true,
                                Arguments = $"--new-window --enable-proposed-api ms-vscode-remote.remote-ssh --remote ssh-remote+{((char)34) + a.Host + ((char)34)}",
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            Process.Start(process);
                            _context.API.LogInfo($"{Name}", $"启动Trae SSH连接成功: {traeExecutable} {process.Arguments}");
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.LogInfo($"{Name}", $"启动Trae SSH连接失败: {ex.Message}");
                            var name = $"{_context.CurrentPluginMetadata.Name}";
                            string msg = "无法启动Trae，请确保Trae已正确安装";
                            _context.API.ShowMsg(name, msg, string.Empty);
                            return false;
                        }
                        catch (Exception ex)
                        {
                            _context.API.LogInfo($"{Name}", $"启动Trae SSH连接时发生未知错误: {ex.Message}");
                            var name = $"{_context.CurrentPluginMetadata.Name}";
                            string msg = "启动Trae时发生错误";
                            _context.API.ShowMsg(name, msg, string.Empty);
                            return false;
                        }
                    },
                    ContextData = a,
                });
            });
            return results;
        }

        private Result CreateWorkspaceResult(TraeWorkspaceItem ws)
        {
            var title = $"{ws.FolderName}";
            var typeWorkspace = ws.WorkspaceTypeToString();

            if (ws.TypeWorkspace != TypeWorkspace.Local)
            {
                title = ws.Label != null
                    ? $"{ws.Label}"
                    : $"{title}{(ws.ExtraInfo != null ? $" - {ws.ExtraInfo}" : string.Empty)} ({typeWorkspace})";
            }

            var tooltip = $"工作区{(ws.TypeWorkspace != TypeWorkspace.Local ? $" 在 {typeWorkspace}" : string.Empty)}: {ws.RelativePath}";

            return new Result
            {
                Title = title,
                SubTitle = tooltip,
                Icon = () => ws.TraeInstance.WorkspaceIcon,
                Action = c =>
                {
                    try
                    {
                        var modifierKeys = c.SpecialKeyState.ToModifierKeys();
                        if (modifierKeys == System.Windows.Input.ModifierKeys.Control)
                        {
                            _context.API.LogInfo($"{Name}", $"打开目录: {ws.RelativePath}");
                            _context.API.OpenDirectory(ws.RelativePath);
                            return true;
                        }

                        var traeExecutable = ws.TraeInstance.ExecutablePath;
                        _context.API.LogInfo($"{Name}", $"使用Trae可执行文件: {traeExecutable}");
                        var process = new ProcessStartInfo
                        {
                            FileName = traeExecutable,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        };
                        process.ArgumentList.Add("--folder-uri");
                        process.ArgumentList.Add(ws.Path);

                        _context.API.LogInfo($"{Name}", $"启动Trae工作区: {traeExecutable} --folder-uri {ws.Path}");
                        Process.Start(process);
                        _context.API.LogInfo($"{Name}", $"启动Trae工作区成功");
                        return true;
                    }
                    catch (Win32Exception ex)
                    {
                        _context.API.LogInfo($"{Name}", $"启动Trae工作区失败: {ex.Message}");
                        var name = $"{_context.CurrentPluginMetadata.Name}";
                        string msg = "无法启动Trae，请确保Trae已正确安装";
                        _context.API.ShowMsg(name, msg, string.Empty);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _context.API.LogInfo($"{Name}", $"启动Trae工作区时发生未知错误: {ex.Message}");
                        var name = $"{_context.CurrentPluginMetadata.Name}";
                        string msg = "启动Trae时发生错误";
                        _context.API.ShowMsg(name, msg, string.Empty);
                        return false;
                    }
                },
                ContextData = ws,
            };
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="context">插件初始化上下文</param>
        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            _context.API.LogInfo($"{Name}", "开始初始化插件");
            _context.API.LogInfo($"{Name}", $"默认实例: {defaultInstance?.ExecutablePath ?? "未找到"}");
            // 传递日志委托给 LoadTraeInstances 方法
            TraeInstances.LoadTraeInstances((sender, message) => _context.API.LogInfo(sender, message));
            defaultInstance = TraeInstances.Instances.FirstOrDefault();
            _context.API.LogInfo($"{Name}", $"找到 {TraeInstances.Instances.Count} 个 Trae 实例");
            foreach (var instance in TraeInstances.Instances)
            {
                _context.API.LogInfo($"{Name}", $"  - 实例: ExecutablePath={instance.ExecutablePath}, AppData={instance.AppData}");
            }
            _context.API.LogInfo($"{Name}", "插件初始化完成");
        }

        /// <summary>
        /// 创建设置面板
        /// </summary>
        /// <returns>设置面板控件</returns>
        public Control CreateSettingPanel() => new SettingsView(_context, _settings);

        /// <summary>
        /// 文化信息改变时调用
        /// </summary>
        /// <param name="newCulture">新的文化信息</param>
        public void OnCultureInfoChanged(CultureInfo newCulture)
        {
        }

        /// <summary>
        /// 获取翻译后的插件标题
        /// </summary>
        /// <returns>插件标题</returns>
        public string GetTranslatedPluginTitle()
        {
            return Name;
        }

        /// <summary>
        /// 获取翻译后的插件描述
        /// </summary>
        /// <returns>插件描述</returns>
        public string GetTranslatedPluginDescription()
        {
            return Description;
        }

        /// <summary>
        /// 加载上下文菜单
        /// </summary>
        /// <param name="selectedResult">选中的结果</param>
        /// <returns>上下文菜单项列表</returns>
        public List<Result> LoadContextMenus(Result selectedResult)
        {
            List<Result> results = new();
            if (selectedResult.ContextData is TraeWorkspaceItem ws && ws.TypeWorkspace == TypeWorkspace.Local)
            {
                results.Add(new Result
                {
                    Title = "打开文件夹",
                    SubTitle = "在文件资源管理器中打开此工作区",
                    Icon = () => ws.TraeInstance.WorkspaceIcon,
                    Action = c =>
                    {
                        _context.API.OpenDirectory(ws.RelativePath);
                        return true;
                    },
                    ContextData = ws,
                });
            }

            return results;
        }
    }
}
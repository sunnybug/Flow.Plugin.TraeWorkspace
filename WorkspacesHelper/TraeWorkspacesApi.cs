using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Flow.Launcher.Plugin;
using Flow.Plugin.TraeWorkspace.TraeHelper;
using Microsoft.Data.Sqlite;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    public class TraeWorkspacesApi
    {
        private readonly string _pluginName = "Trae Workspaces";

        public TraeWorkspacesApi()
        {
        }

        private void LogInfo(string message)
        {
            Main._context?.API.LogInfo(_pluginName, message);
        }

        public static TraeWorkspaceItem ParseVSCodeUri(string uri, TraeInstance traeInstance)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                string unescapeUri = Uri.UnescapeDataString(uri);
                var typeWorkspace = WorkspacesHelper.ParseVSCodeUri.GetTypeWorkspace(unescapeUri);
                if (typeWorkspace.TypeWorkspace.HasValue)
                {
                    var folderName = Path.GetFileName(unescapeUri);

                    // Check we haven't returned '' if we have a path like C:\
                    if (string.IsNullOrEmpty(folderName))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(unescapeUri);
                        folderName = dirInfo.Name.TrimEnd(':');
                    }

                    return new TraeWorkspaceItem()
                    {
                        Path = unescapeUri,
                        RelativePath = typeWorkspace.Path,
                        FolderName = folderName,
                        ExtraInfo = typeWorkspace.MachineName,
                        TypeWorkspace = typeWorkspace.TypeWorkspace.Value,
                        TraeInstance = traeInstance,
                    };
                }
            }

            return null;
        }

        public Regex workspaceLabelParser = new Regex("(.+?)(\\[.+\\])");

        public List<TraeWorkspaceItem> Workspaces
        {
            get
            {
                List<TraeWorkspaceItem> workspaces = new List<TraeWorkspaceItem>();

                foreach (var traeInstance in TraeInstances.Instances)
                {
                    // 确保AppData路径存在
                    if (!Directory.Exists(traeInstance.AppData))
                        continue;

                    // storage.json contains opened Workspaces
                    var traeStorage = Path.Combine(traeInstance.AppData, "storage.json");

                    if (File.Exists(traeStorage))
                    {
                        var fileContent = File.ReadAllText(traeStorage);

                        try
                        {
                            var traeStorageFile = JsonSerializer.Deserialize<VSCodeStorageFile>(fileContent);

                            if (traeStorageFile != null)
                            {
                                // for previous versions of vscode/trae
                                if (traeStorageFile.OpenedPathsList?.Workspaces3 != null)
                                {
                                    workspaces.AddRange(
                                        traeStorageFile.OpenedPathsList.Workspaces3
                                            .Select(workspaceUri => ParseVSCodeUri(workspaceUri, traeInstance))
                                            .Where(uri => uri != null)
                                            .Select(uri => (TraeWorkspaceItem)uri));
                                }

                                // vscode/trae v1.55.0 or later
                                if (traeStorageFile.OpenedPathsList?.Entries != null)
                                {
                                    workspaces.AddRange(traeStorageFile.OpenedPathsList.Entries
                                        .Select(x => x.FolderUri)
                                        .Select(workspaceUri => ParseVSCodeUri(workspaceUri, traeInstance))
                                        .Where(uri => uri != null));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录错误但继续执行
                            Console.WriteLine($"解析Trae存储文件失败: {ex.Message}");
                        }
                    }

                    // for vscode/trae v1.64.0 or later
                    var stateDbPath = Path.Combine(traeInstance.AppData, "User", "globalStorage", "state.vscdb");
                    LogInfo($"检查 state.vscdb 路径: {stateDbPath}");
                    if (File.Exists(stateDbPath))
                    {
                        LogInfo($"找到 state.vscdb 文件: {stateDbPath}");
                        try
                        {
                            LogInfo($"尝试连接 SQLite 数据库: {stateDbPath}");
                            using var connection = new SqliteConnection(
                                $"Data Source={stateDbPath};mode=readonly;cache=shared;");
                            connection.Open();
                            LogInfo("SQLite 数据库连接成功");

                            var command = connection.CreateCommand();
                            command.CommandText = "SELECT value FROM ItemTable where key = 'history.recentlyOpenedPathsList'";
                            LogInfo("执行 SQL 查询: SELECT value FROM ItemTable where key = 'history.recentlyOpenedPathsList'");

                            var result = command.ExecuteScalar();
                            if (result == null)
                            {
                                LogInfo("SQL 查询返回 null，未找到 'history.recentlyOpenedPathsList' 记录");
                                continue;
                            }
                            LogInfo($"SQL 查询成功，返回数据长度: {result.ToString()?.Length ?? 0}");

                            using var historyDoc = JsonDocument.Parse(result.ToString()!);
                            var root = historyDoc.RootElement;
                            if (!root.TryGetProperty("entries", out var entries))
                            {
                                LogInfo("JSON 中未找到 'entries' 属性");
                                continue;
                            }
                            LogInfo($"找到 entries 数组，条目数: {entries.GetArrayLength()}");

                            int validWorkspaceCount = 0;
                            foreach (var entry in entries.EnumerateArray())
                            {
                                if (!entry.TryGetProperty("folderUri", out var folderUri))
                                {
                                    LogInfo("entry 中未找到 'folderUri' 属性，跳过");
                                    continue;
                                }
                                var workspaceUri = folderUri.GetString();
                                LogInfo($"解析 workspace URI: {workspaceUri}");
                                var workspace = ParseVSCodeUri(workspaceUri, traeInstance);
                                if (workspace == null)
                                {
                                    LogInfo($"ParseVSCodeUri 返回 null，跳过: {workspaceUri}");
                                    continue;
                                }
                                validWorkspaceCount++;

                                if (entry.TryGetProperty("label", out var label))
                                {
                                    var labelString = label.GetString()!;
                                    var matchGroup = workspaceLabelParser.Match(labelString);
                                    if (matchGroup.Success)
                                    {
                                        workspace.Label = $"{matchGroup.Groups[2]} {matchGroup.Groups[1]}";
                                    }
                                    else
                                    {
                                        workspace.Label = labelString;
                                    }
                                }

                                workspaces.Add(workspace);
                            }
                            LogInfo($"从 state.vscdb 成功添加 {validWorkspaceCount} 个工作区");
                        }
                        catch (Exception ex)
                        {
                            LogInfo($"解析Trae状态数据库失败: {ex.Message}");
                            LogInfo($"异常详情: {ex}");
                        }
                    }
                    else
                    {
                        LogInfo($"state.vscdb 文件不存在: {stateDbPath}");
                    }
                }

                return workspaces;
            }
        }
    }
}
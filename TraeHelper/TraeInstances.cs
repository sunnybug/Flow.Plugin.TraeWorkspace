using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Flow.Plugin.TraeWorkspace.TraeHelper
{
    /// <summary>
    /// Trae实例管理类
    /// </summary>
    public static class TraeInstances
    {
        private static string _systemPath = string.Empty;

        private static readonly string _userAppDataPath = Environment.GetEnvironmentVariable("AppData");

        private static readonly string _userLocalAppDataPath = Environment.GetEnvironmentVariable("LocalAppData");

        /// <summary>
        /// Trae实例列表
        /// </summary>
        public static List<TraeInstance> Instances { get; set; } = new();

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private static Bitmap BitmapOverlayToCenter(Bitmap bitmap1, Bitmap overlayBitmap)
        {
            int bitmap1Width = bitmap1.Width;
            int bitmap1Height = bitmap1.Height;

            Bitmap overlayBitmapResized = new Bitmap(overlayBitmap, new System.Drawing.Size(bitmap1Width / 2, bitmap1Height / 2));

            float marginLeft = (float)((bitmap1Width * 0.7) - (overlayBitmapResized.Width * 0.5));
            float marginTop = (float)((bitmap1Height * 0.7) - (overlayBitmapResized.Height * 0.5));

            Bitmap finalBitmap = new Bitmap(bitmap1Width, bitmap1Height);
            using (Graphics g = Graphics.FromImage(finalBitmap))
            {
                g.DrawImage(bitmap1, System.Drawing.Point.Empty);
                g.DrawImage(overlayBitmapResized, marginLeft, marginTop);
            }

            return finalBitmap;
        }

        private static string FindTraeExecutable(Action<string, string> logInfo = null)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "trae",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        logInfo?.Invoke("Trae Workspaces", "无法启动 where.exe 进程");
                        return null;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        logInfo?.Invoke("Trae Workspaces", $"where.exe trae 返回错误代码: {process.ExitCode}");
                        return null;
                    }

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        var traePath = lines[0].Trim();
                        logInfo?.Invoke("Trae Workspaces", $"通过 where.exe 找到 Trae 可执行文件: {traePath}");
                        return traePath;
                    }
                    else
                    {
                        logInfo?.Invoke("Trae Workspaces", "where.exe trae 没有找到任何结果");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logInfo?.Invoke("Trae Workspaces", $"执行 where.exe trae 时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载Trae实例
        /// </summary>
        /// <param name="logInfo">日志记录委托</param>
        public static void LoadTraeInstances(Action<string, string> logInfo = null)
        {
            // 总是重新加载Trae实例，确保配置正确
            Instances = new List<TraeInstance>();

            // 首先使用 where.exe 查找 Trae 可执行文件
            var traePath = FindTraeExecutable(logInfo);
            if (!string.IsNullOrEmpty(traePath) && File.Exists(traePath))
            {
                // 根据可执行文件路径判断是否为 Trae CN 版本
                var isTraeCN = traePath.Contains("Trae.CN", StringComparison.OrdinalIgnoreCase) ||
                               traePath.Contains("Trae CN", StringComparison.OrdinalIgnoreCase);
                var appDataFolderName = isTraeCN ? "Trae CN" : "Trae";
                logInfo?.Invoke("Trae Workspaces", $"检测到 Trae 路径: {traePath}, 是否为CN版本: {isTraeCN}, AppData文件夹: {appDataFolderName}");

                var instance = new TraeInstance
                {
                    TraeVersion = TraeVersion.Stable,
                    ExecutablePath = traePath,
                    AppData = Path.Combine(_userAppDataPath, appDataFolderName)
                };

                // 加载图标
                try
                {
                    var bitmapIcon = Icon.ExtractAssociatedIcon(traePath)?.ToBitmap();
                    if (bitmapIcon != null)
                    {
                        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                        // workspace - 使用folder.png作为基础
                        var folderIconPath = Path.Combine(assemblyLocation, "Images", "folder.png");
                        if (File.Exists(folderIconPath))
                        {
                            var folderIcon = (Bitmap)Image.FromFile(folderIconPath);
                            instance.WorkspaceIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(folderIcon, bitmapIcon));
                        }
                        else
                        {
                            instance.WorkspaceIcon = Bitmap2BitmapImage(bitmapIcon);
                        }

                        // remote (SSH) - 使用monitor.png作为基础
                        var monitorIconPath = Path.Combine(assemblyLocation, "Images", "monitor.png");
                        if (File.Exists(monitorIconPath))
                        {
                            var monitorIcon = (Bitmap)Image.FromFile(monitorIconPath);
                            instance.RemoteIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(monitorIcon, bitmapIcon));
                        }
                        else
                        {
                            instance.RemoteIcon = Bitmap2BitmapImage(bitmapIcon);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logInfo?.Invoke("Trae Workspaces", $"加载Trae图标失败: {ex.Message}");
                }

                Instances.Add(instance);
                logInfo?.Invoke("Trae Workspaces", $"添加Trae实例: {instance.ExecutablePath}, AppData: {instance.AppData}");
                logInfo?.Invoke("Trae Workspaces", $"成功加载 {Instances.Count} 个Trae实例");
                return;
            }

            // 如果 where.exe 没有找到，继续使用原来的方法

            _systemPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            logInfo?.Invoke("Trae Workspaces", $"系统PATH环境变量: {_systemPath}");

            // 查找Trae相关路径
            var paths = _systemPath.Split(";").Where(path =>
                path.Contains("Trae", StringComparison.OrdinalIgnoreCase)).ToArray();
            logInfo?.Invoke("Trae Workspaces", $"从PATH中找到的Trae相关路径: {string.Join(", ", paths)}");

            // 检查常见的Trae安装路径
            var commonPaths = new List<string>
            {
                Path.Combine(_userLocalAppDataPath, "Programs", "trae"),
                Path.Combine(_userLocalAppDataPath, "Programs", "Trae"),
                Path.Combine(_userLocalAppDataPath, "Programs", "Trae CN"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Trae"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Trae CN"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Trae"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Trae CN")
            };
            logInfo?.Invoke("Trae Workspaces", $"检查的常见Trae安装路径: {string.Join(", ", commonPaths)}");

            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path) && !paths.Contains(path))
                {
                    paths = paths.Append(path).ToArray();
                    logInfo?.Invoke("Trae Workspaces", $"添加额外的Trae路径: {path}");
                }
            }

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    logInfo?.Invoke("Trae Workspaces", $"路径不存在: {path}");
                    continue;
                }

                var files = Directory.EnumerateFiles(path).Where(file =>
                    file.Contains("trae", StringComparison.OrdinalIgnoreCase) &&
                    file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToArray();
                logInfo?.Invoke("Trae Workspaces", $"在路径 {path} 中找到的Trae可执行文件: {string.Join(", ", files)}");

                if (files.Length <= 0)
                    continue;

                var traeExecFile = files[0];
                var instance = new TraeInstance();

                instance.TraeVersion = TraeVersion.Stable;
                instance.ExecutablePath = traeExecFile;
                logInfo?.Invoke("Trae Workspaces", $"创建Trae实例，可执行文件路径: {traeExecFile}");

                // 确定AppData路径
                var portableData = Path.Join(Path.GetDirectoryName(path), "data");
                if (Directory.Exists(portableData))
                {
                    instance.AppData = Path.Join(portableData, "user-data");
                    logInfo?.Invoke("Trae Workspaces", $"使用便携版AppData路径: {instance.AppData}");
                }
                else
                {
                    // 检查是否为 Trae CN 安装
                    if (path.Contains("Trae CN", StringComparison.OrdinalIgnoreCase))
                    {
                        instance.AppData = Path.Combine(_userAppDataPath, "Trae CN");
                        logInfo?.Invoke("Trae Workspaces", $"使用Trae CN AppData路径: {instance.AppData}");
                    }
                    else
                    {
                        instance.AppData = Path.Combine(_userAppDataPath, "Trae");
                        logInfo?.Invoke("Trae Workspaces", $"使用Trae AppData路径: {instance.AppData}");
                    }
                }

                // 加载图标
                try
                {
                    var bitmapIcon = Icon.ExtractAssociatedIcon(traeExecFile)?.ToBitmap();
                    if (bitmapIcon != null)
                    {
                        // 使用与CursorWorkspace相同的图标叠加方法
                        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                        // workspace - 使用folder.png作为基础
                        var folderIconPath = Path.Combine(assemblyLocation, "Images", "folder.png");
                        if (File.Exists(folderIconPath))
                        {
                            var folderIcon = (Bitmap)Image.FromFile(folderIconPath);
                            instance.WorkspaceIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(folderIcon, bitmapIcon));
                        }
                        else
                        {
                            // 如果没有folder.png，直接使用Trae图标
                            instance.WorkspaceIcon = Bitmap2BitmapImage(bitmapIcon);
                        }

                        // remote (SSH) - 使用monitor.png作为基础
                        var monitorIconPath = Path.Combine(assemblyLocation, "Images", "monitor.png");
                        if (File.Exists(monitorIconPath))
                        {
                            var monitorIcon = (Bitmap)Image.FromFile(monitorIconPath);
                            instance.RemoteIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(monitorIcon, bitmapIcon));
                        }
                        else
                        {
                            // 如果没有monitor.png，直接使用Trae图标
                            instance.RemoteIcon = Bitmap2BitmapImage(bitmapIcon);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续执行
                    logInfo?.Invoke("Trae Workspaces", $"加载Trae图标失败: {ex.Message}");
                }

                Instances.Add(instance);
                logInfo?.Invoke("Trae Workspaces", $"添加Trae实例: {instance.ExecutablePath}, AppData: {instance.AppData}");
            }

            // 如果没有找到实例，不添加默认实例
            if (Instances.Count == 0)
            {
                logInfo?.Invoke("Trae Workspaces", "没有找到Trae实例");
            }
            else
            {
                logInfo?.Invoke("Trae Workspaces", $"成功加载 {Instances.Count} 个Trae实例");
            }
        }
    }
}
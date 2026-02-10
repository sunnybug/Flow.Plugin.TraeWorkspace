using System.Collections.Generic;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    /// <summary>
    /// VSCode存储文件类
    /// </summary>
    public class VSCodeStorageFile
    {
        /// <summary>
        /// 已打开路径列表
        /// </summary>
        public OpenedPathsList OpenedPathsList { get; set; }
    }

    /// <summary>
    /// 已打开路径列表类
    /// </summary>
    public class OpenedPathsList
    {
        /// <summary>
        /// 工作区列表（旧版本）
        /// </summary>
        public List<string> Workspaces3 { get; set; }

        /// <summary>
        /// 工作区条目列表
        /// </summary>
        public List<VSCodeWorkspaceEntry> Entries { get; set; }
    }

    /// <summary>
    /// VSCode工作区条目类
    /// </summary>
    public class VSCodeWorkspaceEntry
    {
        /// <summary>
        /// 文件夹URI
        /// </summary>
        public string FolderUri { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }
    }
}
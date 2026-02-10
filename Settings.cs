using System.Collections.Generic;

namespace Flow.Plugin.TraeWorkspace
{
    /// <summary>
    /// 插件设置类
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// 是否自动发现工作区
        /// </summary>
        public bool DiscoverWorkspaces { get; set; } = true;

        /// <summary>
        /// 是否自动发现远程机器
        /// </summary>
        public bool DiscoverMachines { get; set; } = true;

        /// <summary>
        /// 自定义工作区列表
        /// </summary>
        public List<string> CustomWorkspaces { get; set; } = new();
    }
}
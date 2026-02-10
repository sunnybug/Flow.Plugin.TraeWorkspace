using Flow.Plugin.TraeWorkspace.TraeHelper;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    /// <summary>
    /// 工作区类型枚举
    /// </summary>
    public enum TypeWorkspace
    {
        /// <summary>
        /// 本地工作区
        /// </summary>
        Local = 1,

        /// <summary>
        /// 远程工作区
        /// </summary>
        Remote = 2,

        /// <summary>
        /// 容器工作区
        /// </summary>
        Container = 3
    }

    /// <summary>
    /// Trae工作区项类
    /// </summary>
    public class TraeWorkspaceItem
    {
        /// <summary>
        /// 工作区路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 额外信息
        /// </summary>
        public string ExtraInfo { get; set; }

        /// <summary>
        /// 工作区类型
        /// </summary>
        public TypeWorkspace TypeWorkspace { get; set; }

        /// <summary>
        /// 关联的Trae实例
        /// </summary>
        public TraeInstance TraeInstance { get; set; }

        /// <summary>
        /// 将工作区类型转换为字符串
        /// </summary>
        /// <returns>工作区类型字符串</returns>
        public string WorkspaceTypeToString()
        {
            return TypeWorkspace switch
            {
                TypeWorkspace.Local => "Local",
                TypeWorkspace.Remote => "Remote",
                TypeWorkspace.Container => "Container",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 判断两个工作区项是否相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is not TraeWorkspaceItem other)
                return false;

            return Path == other.Path;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return Path?.GetHashCode() ?? 0;
        }
    }
}
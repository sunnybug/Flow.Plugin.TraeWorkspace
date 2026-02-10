using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flow.Plugin.TraeWorkspace.TraeHelper
{
    /// <summary>
    /// Trae版本枚举
    /// </summary>
    public enum TraeVersion
    {
        /// <summary>
        /// 稳定版
        /// </summary>
        Stable = 1,

        /// <summary>
        /// 预览版
        /// </summary>
        Insiders = 2
    }

    /// <summary>
    /// Trae实例类
    /// </summary>
    public class TraeInstance : IEquatable<TraeInstance>
    {
        /// <summary>
        /// Trae版本
        /// </summary>
        public TraeVersion TraeVersion { get; set; }

        /// <summary>
        /// AppData路径
        /// </summary>
        public string AppData { get; set; } = string.Empty;

        /// <summary>
        /// 可执行文件路径
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;

        /// <summary>
        /// 工作区图标
        /// </summary>
        public ImageSource WorkspaceIcon { get; set; }

        /// <summary>
        /// 远程连接图标
        /// </summary>
        public ImageSource RemoteIcon { get; set; }

        /// <summary>
        /// 判断两个Trae实例是否相等
        /// </summary>
        /// <param name="other">要比较的实例</param>
        /// <returns>是否相等</returns>
        public bool Equals(TraeInstance other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return TraeVersion == other.TraeVersion
                   && string.Equals(AppData, other.AppData, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is TraeInstance instance && Equals(instance);
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)TraeVersion,
                AppData.GetHashCode(StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
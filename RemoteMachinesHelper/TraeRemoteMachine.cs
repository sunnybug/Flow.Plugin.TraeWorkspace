using Flow.Plugin.TraeWorkspace.TraeHelper;

namespace Flow.Plugin.TraeWorkspace.RemoteMachinesHelper
{
    /// <summary>
    /// Trae远程机器信息类
    /// </summary>
    public class TraeRemoteMachine
    {
        /// <summary>
        /// SSH主机别名
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SSH用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// SSH主机名或IP地址
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 关联的Trae实例
        /// </summary>
        public TraeInstance TraeInstance { get; set; }

        /// <summary>
        /// 判断两个远程机器是否相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is not TraeRemoteMachine other)
                return false;

            return Host == other.Host;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return Host?.GetHashCode() ?? 0;
        }
    }
}
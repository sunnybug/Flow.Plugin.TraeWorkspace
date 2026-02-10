using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flow.Plugin.TraeWorkspace.SshConfigParser
{
    /// <summary>
    /// SSH配置类
    /// </summary>
    public class SshConfig
    {
        /// <summary>
        /// SSH主机列表
        /// </summary>
        public List<SshHost> Hosts { get; set; } = new();
    }

    /// <summary>
    /// SSH主机配置类
    /// </summary>
    public class SshHost
    {
        /// <summary>
        /// 主机别名
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 主机名或IP地址
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 身份验证文件路径
        /// </summary>
        public string IdentityFile { get; set; }
    }

    /// <summary>
    /// SSH配置文件解析器
    /// </summary>
    public static class SshConfigParser
    {
        /// <summary>
        /// 解析SSH配置文件
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <returns>SSH配置对象</returns>
        public static SshConfig Parse(string filePath)
        {
            var config = new SshConfig();
            SshHost currentHost = null;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmedLine = line.Trim();
                
                // 跳过空行和注释
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // 检查是否是新的Host条目
                if (trimmedLine.StartsWith("Host ", StringComparison.OrdinalIgnoreCase))
                {
                    currentHost = new SshHost
                    {
                        Host = trimmedLine.Substring("Host ".Length).Trim()
                    };
                    config.Hosts.Add(currentHost);
                }
                // 否则是Host的属性
                else if (currentHost != null && trimmedLine.Contains(' '))
                {
                    var parts = trimmedLine.Split(new[] { ' ' }, 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim().ToLower();
                        var value = parts[1].Trim();

                        switch (key)
                        {
                            case "hostname":
                                currentHost.HostName = value;
                                break;
                            case "user":
                                currentHost.User = value;
                                break;
                            case "port":
                                currentHost.Port = value;
                                break;
                            case "identityfile":
                                currentHost.IdentityFile = value;
                                break;
                        }
                    }
                }
            }

            return config;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Plugin.TraeWorkspace.TraeHelper;
using Flow.Plugin.TraeWorkspace.SshConfigParser;

namespace Flow.Plugin.TraeWorkspace.RemoteMachinesHelper
{
    public class TraeRemoteMachinesApi
    {
        public TraeRemoteMachinesApi()
        {
        }

        public List<TraeRemoteMachine> Machines
        {
            get
            {
                var machines = new List<TraeRemoteMachine>();

                foreach (var traeInstance in TraeInstances.Instances)
                {
                    // 尝试从SSH配置文件加载远程机器
                    var sshConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
                    if (File.Exists(sshConfigPath))
                    {
                        try
                        {
                            var sshConfig = SshConfigParser.SshConfigParser.Parse(sshConfigPath);
                            foreach (var host in sshConfig.Hosts)
                            {
                                // 跳过包含通配符的 Host 条目（如 Host *）
                                if (host.Host.Contains("*") || host.Host.Contains("?"))
                                    continue;

                                // 跳过 Git 托管平台配置（User 为 git 且 Hostname 为常见 Git 平台）
                                if (!string.IsNullOrEmpty(host.HostName) &&
                                    !string.IsNullOrEmpty(host.User) &&
                                    host.User.Equals("git", StringComparison.OrdinalIgnoreCase))
                                {
                                    var hostNameLower = host.HostName.ToLower();
                                    // 常见 Git 托管平台列表
                                    var gitPlatforms = new[]
                                    {
                                        "github.com",
                                        "gitee.com",
                                        "gitlab.com",
                                        "bitbucket.org",
                                        "coding.net",
                                        "code.aliyun.com",
                                        "dev.azure.com",
                                        "ssh.dev.azure.com",
                                        "sourceforge.net",
                                        "gitcode.com"
                                    };
                                    if (gitPlatforms.Any(platform => hostNameLower.EndsWith(platform)))
                                        continue;
                                }

                                machines.Add(new TraeRemoteMachine
                                {
                                    Host = host.Host,
                                    User = host.User,
                                    HostName = host.HostName,
                                    TraeInstance = traeInstance
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录错误但继续执行
                            Console.WriteLine($"解析SSH配置文件失败: {ex.Message}");
                        }
                    }
                }

                // 去重
                return machines.Distinct().ToList();
            }
        }
    }
}
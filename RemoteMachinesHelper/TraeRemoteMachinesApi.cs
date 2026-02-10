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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flow.Plugin.TraeWorkspace.SshConfigParser
{
    public class SshConfig
    {
        public List<SshHost> Hosts { get; set; } = new();
    }

    public class SshHost
    {
        public string Host { get; set; }

        public string HostName { get; set; }

        public string User { get; set; }

        public string Port { get; set; }

        public string IdentityFile { get; set; }
    }

    public static class SshConfigParser
    {
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
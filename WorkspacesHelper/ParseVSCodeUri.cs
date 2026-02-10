using System;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    /// <summary>
    /// VSCode URI解析类
    /// </summary>
    public class ParseVSCodeUri
    {
        /// <summary>
        /// 获取工作区类型信息
        /// </summary>
        /// <param name="uri">VSCode URI</param>
        /// <returns>路径、机器名和工作区类型</returns>
        public static (string Path, string MachineName, TypeWorkspace? TypeWorkspace) GetTypeWorkspace(string uri)
        {
            if (uri.StartsWith("vscode-remote://"))
            {
                // Format: vscode-remote://ssh-remote+hostname/path
                // Format: vscode-remote://containers+hash/path
                var parts = uri.Split(new[] { '/' }, 4);
                if (parts.Length >= 4)
                {
                    var remotePart = parts[2];
                    var path = string.Join("/", parts[3..]);
                    
                    if (remotePart.StartsWith("ssh-remote+"))
                    {
                        var machineName = remotePart.Substring("ssh-remote+".Length);
                        return (path, machineName, TypeWorkspace.Remote);
                    }
                    else if (remotePart.StartsWith("containers+"))
                    {
                        var machineName = remotePart.Substring("containers+".Length);
                        return (path, machineName, TypeWorkspace.Container);
                    }
                }
            }
            else if (uri.StartsWith("file://"))
            {
                // Format: file:///c:/path or file:///home/user/path
                var path = uri.Substring("file://".Length);
                // 处理Windows路径
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                // 将正斜杠替换为反斜杠（Windows）
                path = path.Replace('/', '\\');
                return (path, null, TypeWorkspace.Local);
            }
            else
            {
                // 本地路径
                return (uri, null, TypeWorkspace.Local);
            }

            return (null, null, null);
        }
    }
}
using System.Collections.Generic;

namespace Flow.Plugin.TraeWorkspace
{
    public class Settings
    {
        public bool DiscoverWorkspaces { get; set; } = true;

        public bool DiscoverMachines { get; set; } = true;

        public List<string> CustomWorkspaces { get; set; } = new();
    }
}
using System.Collections.Generic;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    public class VSCodeStorageFile
    {
        public OpenedPathsList OpenedPathsList { get; set; }
    }

    public class OpenedPathsList
    {
        public List<string> Workspaces3 { get; set; }

        public List<VSCodeWorkspaceEntry> Entries { get; set; }
    }

    public class VSCodeWorkspaceEntry
    {
        public string FolderUri { get; set; }

        public string Label { get; set; }
    }
}
using Flow.Plugin.TraeWorkspace.TraeHelper;

namespace Flow.Plugin.TraeWorkspace.WorkspacesHelper
{
    public enum TypeWorkspace
    {
        Local = 1,
        Remote = 2,
        Container = 3
    }

    public class TraeWorkspaceItem
    {
        public string Path { get; set; }

        public string RelativePath { get; set; }

        public string FolderName { get; set; }

        public string Label { get; set; }

        public string ExtraInfo { get; set; }

        public TypeWorkspace TypeWorkspace { get; set; }

        public TraeInstance TraeInstance { get; set; }

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

        public override bool Equals(object obj)
        {
            if (obj is not TraeWorkspaceItem other)
                return false;

            return Path == other.Path;
        }

        public override int GetHashCode()
        {
            return Path?.GetHashCode() ?? 0;
        }
    }
}
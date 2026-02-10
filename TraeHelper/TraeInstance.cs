using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flow.Plugin.TraeWorkspace.TraeHelper
{
    public enum TraeVersion
    {
        Stable = 1,
        Insiders = 2
    }

    public class TraeInstance : IEquatable<TraeInstance>
    {
        public TraeVersion TraeVersion { get; set; }

        public string AppData { get; set; } = string.Empty;

        public string ExecutablePath { get; set; } = string.Empty;

        public ImageSource WorkspaceIcon { get; set; }

        public ImageSource RemoteIcon { get; set; }

        public bool Equals(TraeInstance other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return TraeVersion == other.TraeVersion
                   && string.Equals(AppData, other.AppData, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is TraeInstance instance && Equals(instance);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)TraeVersion,
                AppData.GetHashCode(StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
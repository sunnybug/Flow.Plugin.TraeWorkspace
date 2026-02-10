using Flow.Plugin.TraeWorkspace.TraeHelper;

namespace Flow.Plugin.TraeWorkspace.RemoteMachinesHelper
{
    public class TraeRemoteMachine
    {
        public string Host { get; set; }

        public string User { get; set; }

        public string HostName { get; set; }

        public TraeInstance TraeInstance { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not TraeRemoteMachine other)
                return false;

            return Host == other.Host;
        }

        public override int GetHashCode()
        {
            return Host?.GetHashCode() ?? 0;
        }
    }
}
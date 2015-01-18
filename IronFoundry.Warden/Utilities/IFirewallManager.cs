namespace IronFoundry.Warden.Utilities
{
    // BR: Move this to IronFoundry.Container
    public interface IFirewallManager
    {
        void OpenPort(ushort port, string name);
        void ClosePort(string name);
    }
}
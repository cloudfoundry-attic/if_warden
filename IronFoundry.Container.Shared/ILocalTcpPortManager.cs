namespace IronFoundry.Warden.Utilities
{
    // BR: Move this to IronFoundry.Container
    public interface ILocalTcpPortManager
    {
        int ReserveLocalPort(int port, string userName);
        void ReleaseLocalPort(ushort? port, string userName);
    }
}
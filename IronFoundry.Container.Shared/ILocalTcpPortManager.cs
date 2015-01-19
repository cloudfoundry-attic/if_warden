namespace IronFoundry.Warden.Utilities
{
    // BR: Move this to IronFoundry.Container
    public interface ILocalTcpPortManager
    {
        ushort ReserveLocalPort(ushort port, string userName);
        void ReleaseLocalPort(ushort? port, string userName);
    }
}
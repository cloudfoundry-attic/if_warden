namespace IronFoundry.Warden.Utilities
{
    // BR: Move this to IronFoundry.Container
    public interface INetShRunner
    {
        bool AddRule(ushort port, string userName);
        bool DeleteRule(ushort port);
    }
}
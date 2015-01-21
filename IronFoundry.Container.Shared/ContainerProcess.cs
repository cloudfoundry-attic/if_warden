using System;
using System.Diagnostics;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    // BR: Move this to IronFoundry.Container.Shared
    public class ContainerProcess
    {
        readonly IProcess process;

        public ContainerProcess(IProcess process)
        {
            this.process = process;
        }

        int Id
        {
            get { return process.Id; }
        }

        void Kill()
        {
            process.Kill();
        }

        int WaitForExit()
        {
            process.WaitForExit();
            return process.ExitCode;
        }

        //int WaitForExit(int milliseconds);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    public class ProcessSpec
    {
        public string ExecutablePath { get; set; }
        public string[] Arguments { get; set; }
        public Dictionary<string, string> Environment { get; set; }
        public string WorkingDirectory { get; set; }
        public bool Privileged { get; set; }
    }

    public interface IProcessIO
    {
        TextWriter StandardOutput { get; }
        TextWriter StandardError { get; }
        TextReader StandardInput { get; }
    }

    public interface IContainer
    {
        //string ContainerDirectoryPath { get; }
        //string ContainerUserName { get; }
        string Handle { get; }
        //ContainerState State { get; }

        //void BindMounts(IEnumerable<BindMount> mounts);
        //void CreateTarFile(string sourcePath, string tarFilePath, bool compress);
        //void Copy(string source, string destination);
        //void CopyFileIn(string sourceFilePath, string destinationFilePath);
        //void CopyFileOut(string sourceFilePath, string destinationFilePath);
        //void ExtractTarFile(string tarFilePath, string destinationPath, bool decompress);

        //IProcess CreateProcess(CreateProcessStartInfo si, bool impersonate = false);
        //WindowsImpersonationContext GetExecutionContext(bool shouldImpersonate = false);
        //ContainerInfo GetInfo();

        //void Initialize(IContainerDirectory containerDirectory, ContainerHandle containerHandle, IContainerUser userInfo);
        //void Stop(bool kill);

        int ReservePort(int requestedPort);
        ContainerProcess Run(ProcessSpec spec, IProcessIO io);
    }

    public class Container : IContainer
    {
        readonly string handle;
        readonly IContainerUser user;
        readonly IContainerDirectory directory;
        readonly ILocalTcpPortManager tcpPortManager;
        readonly IProcessRunner localProcessRunner;
        readonly IProcessRunner remoteProcessRunner;

        public Container(
            string handle,
            IContainerUser user,
            IContainerDirectory directory, 
            ILocalTcpPortManager tcpPortManager,
            IProcessRunner localProcessRunner,
            IProcessRunner remoteProcessRunner
            )
        {
            this.handle = handle;
            this.user = user;
            this.directory = directory;
            this.tcpPortManager = tcpPortManager;
            this.localProcessRunner = localProcessRunner;
            this.remoteProcessRunner = remoteProcessRunner;
        }

        public string Handle
        {
            get { return handle; }
        }

        public void Initialize()
        {
            // Start the 'host' process
            // Initialize the host (or wait for the host to initialize if it's implicit)
        }

        public int ReservePort(int requestedPort)
        {
            return tcpPortManager.ReserveLocalPort(requestedPort, user.UserName);
        }

        public ContainerProcess Run(ProcessSpec spec, IProcessIO io)
        {
            throw new NotImplementedException();
        }
    }
}

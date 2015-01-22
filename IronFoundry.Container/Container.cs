using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Container.Messaging;
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
        string Handle { get; }
        //ContainerState State { get; }

        //void BindMounts(IEnumerable<BindMount> mounts);
        //void CreateTarFile(string sourcePath, string tarFilePath, bool compress);
        //void CopyFileIn(string sourceFilePath, string destinationFilePath);
        //void CopyFileOut(string sourceFilePath, string destinationFilePath);
        //void ExtractTarFile(string tarFilePath, string destinationPath, bool decompress);

        //ContainerInfo GetInfo();

        //void Stop(bool kill);

        int ReservePort(int requestedPort);
        ContainerProcess Run(ProcessSpec spec, IProcessIO io);


        //void Initialize(IContainerDirectory containerDirectory, ContainerHandle containerHandle, IContainerUser userInfo);
        //string ContainerDirectoryPath { get; }
        //string ContainerUserName { get; }
        //void Copy(string source, string destination);
    }

    public class Container : IContainer
    {
        const string DefaultWorkingDirectory = "/";

        readonly string handle;
        readonly IContainerUser user;
        readonly IContainerDirectory directory;
        readonly ILocalTcpPortManager tcpPortManager;
        readonly IProcessRunner processRunner;
        readonly IProcessRunner constrainedProcessRunner;
        readonly Dictionary<string, string> defaultEnvironment;

        public Container(
            string handle,
            IContainerUser user,
            IContainerDirectory directory, 
            ILocalTcpPortManager tcpPortManager,
            IProcessRunner processRunner,
            IProcessRunner constrainedProcessRunner
            )
        {
            this.handle = handle;
            this.user = user;
            this.directory = directory;
            this.tcpPortManager = tcpPortManager;
            this.processRunner = processRunner;
            this.constrainedProcessRunner = constrainedProcessRunner;

            this.defaultEnvironment = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
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
            var runner = spec.Privileged ?
                processRunner :
                constrainedProcessRunner;

            var runSpec = new ProcessRunSpec
            {
                ExecutablePath = directory.MapUserPath(spec.ExecutablePath),
                Arguments = spec.Arguments,
                Environment = spec.Environment ?? defaultEnvironment,
                WorkingDirectory = directory.MapUserPath(spec.WorkingDirectory ?? DefaultWorkingDirectory),
                OutputCallback = data => io.StandardOutput.Write(data),
                ErrorCallback = data => io.StandardError.Write(data),
            };

            var process = runner.Run(runSpec);

            return new ContainerProcess(process);
        }
    }
}
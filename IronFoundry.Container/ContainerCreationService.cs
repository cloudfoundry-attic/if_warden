using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    public class ContainerUser : IContainerUser
    {
        readonly IUserManager userManager; // TODO: Refactor this out of this class
        readonly NetworkCredential credentials;

        public ContainerUser(IUserManager userManager, NetworkCredential credentials)
        {
            this.userManager = userManager;
            this.credentials = credentials;
        }

        public string UserName
        {
            get { return credentials.UserName; }
        }

        public NetworkCredential GetCredential()
        {
            return credentials;
        }

        static string BuildContainerUserName(string handle)
        {
            return "container_" + handle;
        }

        public static ContainerUser Create(IUserManager userManager, string containerHandle)
        {
            var credentials = userManager.CreateUser(BuildContainerUserName(containerHandle));
            return new ContainerUser(userManager, credentials);
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }

    public class ContainerSpec
    {
        public string Handle { get; set; }
        public BindMount[] BindMounts { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public Dictionary<string, string> Environment { get; set; }
    }

    public interface IContainerCreationService : IDisposable
    {
        IContainer CreateContainer(ContainerSpec containerSpec);
    }

    public class ContainerCreationService : IContainerCreationService
    {
        readonly string containerBasePath;
        readonly FileSystemManager fileSystem;
        readonly IUserManager userManager;
        readonly ILocalTcpPortManager tcpPortManager;
        readonly IProcessRunner localProcessRunner;
        readonly IProcessRunner remoteProcessRunner;

        public ContainerCreationService(
            IUserManager userManager,
            FileSystemManager fileSystem,
            ILocalTcpPortManager tcpPortManager,
            IProcessRunner localProcessRunner,
            IProcessRunner remoteProcessRunner,
            string containerBasePath
            )
        {
            this.userManager = userManager;
            this.fileSystem = fileSystem;
            this.tcpPortManager = tcpPortManager;
            this.localProcessRunner = localProcessRunner;
            this.remoteProcessRunner = remoteProcessRunner;
            this.containerBasePath = containerBasePath;
        }

        public IContainer CreateContainer(ContainerSpec containerSpec)
        {
            Guard.NotNull(containerSpec, "containerSpec");

            var handle = containerSpec.Handle;
            if (String.IsNullOrEmpty(handle))
                handle = ContainerHandleGenerator.Generate();

            var user = ContainerUser.Create(userManager, handle);
            var directory = ContainerDirectory.Create(fileSystem, containerBasePath, handle, user);

            return new Container(handle, user, directory, tcpPortManager, localProcessRunner, remoteProcessRunner);
        }

        public void Dispose()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Container;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    public interface IContainerDirectory
    {
    }

    public class ContainerDirectory : IContainerDirectory
    {
        readonly string containerPath;

        public ContainerDirectory(string containerPath)
        {
            this.containerPath = containerPath;
        }

        public static ContainerDirectory Create(FileSystemManager fileSystem, string containerBasePath, string containerHandle, IContainerUser containerUser)
        {
            // TODO: Sanitize the container handle for use in the filesystem
            var containerPath = Path.Combine(containerBasePath, containerHandle);
            var userAccess = Enumerable.Empty<UserAccess>();

            fileSystem.CreateDirectory(containerPath, userAccess);

            return new ContainerDirectory(containerPath);
        }
    }
}
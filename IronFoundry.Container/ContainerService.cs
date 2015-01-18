using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronFoundry.Container
{
    public interface IContainerService : IDisposable
    {
        IContainer CreateContainer(string handle);
    }

    public class ContainerService : IContainerService
    {
        readonly string containerBasePath;
        readonly string containerUsersGroupName;

        public ContainerService(string containerBasePath, string containerUsersGroupName)
        {
            this.containerBasePath = containerBasePath;
            this.containerUsersGroupName = containerUsersGroupName;
        }

        public IContainer CreateContainer(string handle)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}

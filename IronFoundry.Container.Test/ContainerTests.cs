using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    public class ContainerTests
    {
        Container Container { get; set; }
        IContainerUser User { get; set; }
        IContainerDirectory Directory { get; set; }
        IProcessRunner LocalProcessRunner { get; set; }
        IProcessRunner RemoteProcessRunner { get; set; }
        ILocalTcpPortManager TcpPortManager { get; set; }

        public ContainerTests()
        {
            User = Substitute.For<IContainerUser>();
            User.UserName.Returns("container-username");
            Directory = Substitute.For<IContainerDirectory>();
            LocalProcessRunner = Substitute.For<IProcessRunner>();
            RemoteProcessRunner = Substitute.For<IProcessRunner>();
            TcpPortManager = Substitute.For<ILocalTcpPortManager>();

            Container = new Container("handle", User, Directory, TcpPortManager, LocalProcessRunner, RemoteProcessRunner);
        }

        public class ReservePort : ContainerTests
        {
            [Fact]
            public void ReservesPortForContainerUser()
            {
                Container.ReservePort(3000);

                TcpPortManager.Received(1).ReserveLocalPort(3000, "container-username");
            }

            [Fact]
            public void ReturnsReservedPort()
            {
                TcpPortManager.ReserveLocalPort(3000, "container-username").Returns(5000);

                var port = Container.ReservePort(3000);

                Assert.Equal(5000, port);
            }
        }

        public class Run_Privileged : ContainerTests
        {
            [Fact]
            public void RunsTheProcessLocally()
            {
                throw new NotImplementedException();
            }

            [Fact]
            public void ProcessOutputEventsAreWrittenToStandardOutput()
            {
                throw new NotImplementedException();
            }

            [Fact]
            public void ProcessErrorEventsAreWrittenToStandardError()
            {
                throw new NotImplementedException();
            }
        }
    }
}

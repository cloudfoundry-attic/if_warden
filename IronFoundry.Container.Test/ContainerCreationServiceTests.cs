using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Warden.Utilities;
using NSubstitute;
using Xunit;

namespace IronFoundry.Container
{
    public class ContainerCreationServiceTests
    {
        string ContainerBasePath { get; set; }
        FileSystemManager FileSystem { get; set; }
        IUserManager UserManager { get; set; }
        ContainerCreationService Service { get; set; }

        public ContainerCreationServiceTests()
        {
            ContainerBasePath = @"C:\Containers";

            FileSystem = Substitute.For<FileSystemManager>();
            UserManager = Substitute.For<IUserManager>();
            Service = new ContainerCreationService(UserManager, FileSystem, ContainerBasePath);
        }

        public class CreateContainer : ContainerCreationServiceTests
        {
            [Fact]
            public void WhenSpecIsNull_Throws()
            {
                var ex = Record.Exception(() => Service.CreateContainer(null));

                Assert.IsAssignableFrom<ArgumentException>(ex);
            }

            [Fact]
            public void UsesProvidedHandle()
            {
                var spec = new ContainerSpec
                {
                    Handle = "container-handle",
                };

                var container = Service.CreateContainer(spec);

                Assert.Equal("container-handle", container.Handle);
            }

            [InlineData(null)]
            [InlineData("")]
            [Theory]
            public void WhenHandleIsNotProvided_GeneratesHandle(string handle)
            {
                var spec = new ContainerSpec
                {
                    Handle = handle,
                };

                var container = Service.CreateContainer(spec);

                Assert.NotNull(container.Handle);
                Assert.NotEmpty(container.Handle);
            }
            
            [Fact]
            public void CreatesContainerSpecificUser()
            {
                UserManager.CreateUser("").ReturnsForAnyArgs(new NetworkCredential());
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                Service.CreateContainer(spec);

                UserManager.Received(1).CreateUser("container_handle");
            }

            [Fact]
            public void CreatesContainerSpecificDirectory()
            {
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                Service.CreateContainer(spec);

                var expectedPath = Path.Combine(ContainerBasePath, "handle");
                FileSystem.Received(1).CreateDirectory(expectedPath, Arg.Any<IEnumerable<UserAccess>>());
            }
        }
    }
}

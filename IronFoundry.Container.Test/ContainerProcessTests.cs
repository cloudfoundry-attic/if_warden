using IronFoundry.Warden.Utilities;
using NSubstitute;
using Xunit;

namespace IronFoundry.Container
{
    public class ContainerProcessTests
    {
        IProcess Process { get; set; }
        ContainerProcess ContainerProcess { get; set; }

        public ContainerProcessTests()
        {
            Process = Substitute.For<IProcess>();

            ContainerProcess = new ContainerProcess(Process);
        }

        public class Id : ContainerProcessTests
        {
            [Fact]
            public void ReturnsProcessId()
            {
                Process.Id.Returns(100);

                Assert.Equal(100, ContainerProcess.Id);
            }
        }

        public class Kill : ContainerProcessTests
        {
            [Fact]
            public void KillsProcess()
            {
                ContainerProcess.Kill();

                Process.Received(1).Kill();
            }
        }

        public class WaitForExit : ContainerProcessTests
        {
            [Fact]
            public void WaitsForProcessToExitAndReturnsExitCode()
            {
                Process.ExitCode.Returns(1);

                var exitCode = ContainerProcess.WaitForExit();

                Assert.Equal(1, exitCode);
                Process.Received(1).WaitForExit();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace IronFoundry.Container.Utilities
{
    public class ProcessHelperTest
    {
        public ProcessStartInfo CmdExeStartInfo { get; set; }

        public ProcessHelperTest()
        {
            CmdExeStartInfo = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.SystemDirectory,
            };
        }
        

        public class GetProcessById : ProcessHelperTest
        {
            [Fact]
            public void WhenProcessIsValid_ReturnsProcess()
            {
                Process p = Process.Start(CmdExeStartInfo);
                try
                {
                    var processHelper = new ProcessHelper();

                    var result = processHelper.GetProcessById(p.Id);

                    Assert.NotNull(result);
                    Assert.Equal(p.Id, result.Id);
                }
                finally
                {
                    p.Kill();
                }
            }

            [Fact]
            public void WhenProcessDoesNotExist_ReturnsNull()
            {
                var processHelper = new ProcessHelper();

                var result = processHelper.GetProcessById(1);

                Assert.Null(result);
            }
        }

        public class GetProcesses : ProcessHelperTest
        {
            [Fact]
            public void ReturnsProcesses()
            {
                var processes = new []
                {
                    Process.Start(CmdExeStartInfo),
                    Process.Start(CmdExeStartInfo),
                };

                try
                {
                    var processHelper = new ProcessHelper();

                    var result = processHelper.GetProcesses(processes.Select(p => p.Id));

                    Assert.Collection(result,
                        x => Assert.Equal(processes[0].Id, x.Id),
                        x => Assert.Equal(processes[1].Id, x.Id)
                    );
                }
                finally
                {
                    foreach (var p in processes)
                        p.Kill();
                }
            }

            [Fact]
            public void FiltersOutMissingProcesses()
            {
                var processes = new[]
                {
                    Process.Start(CmdExeStartInfo),
                    Process.Start(CmdExeStartInfo),
                };

                try
                {
                    var processHelper = new ProcessHelper();

                    var processIds = processes.Select(p => p.Id).ToList();
                    processIds.Add(1);

                    var result = processHelper.GetProcesses(processes.Select(p => p.Id));

                    Assert.Collection(result,
                        x => Assert.Equal(processes[0].Id, x.Id),
                        x => Assert.Equal(processes[1].Id, x.Id)
                    );
                }
                finally
                {
                    foreach (var p in processes)
                        p.Kill();
                }
            }
        }
    }
}

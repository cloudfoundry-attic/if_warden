using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IronFoundry.Container.Utilities;
using Xunit;

namespace IronFoundry.Container.Acceptance
{
    public class ContainerAcceptanceTests : IDisposable
    {
        string Container1Handle { get; set; }
        string Container2Handle { get; set; }
        string UserGroupName { get; set; }
        string ContainerBasePath { get; set; }
        string ReadOnlyBindMountPath { get; set; }
        string ReadWriteBindMountPath { get; set; }

        LocalUserGroupManager UserGroupManager { get; set; }
        IContainerService ContainerService { get; set; }
        IContainer Container1 { get; set; }
        IContainer Container2 { get; set; }

        public ContainerAcceptanceTests()
        {
            Container1Handle = GenerateRandomAlphaString();
            Container2Handle = GenerateRandomAlphaString();
            ContainerBasePath = CreateTempDirectory();
            ReadOnlyBindMountPath = CreateTempDirectory();
            ReadWriteBindMountPath = CreateTempDirectory();

            UserGroupName = "ContainerServiceTestsUserGroup_" + GenerateRandomAlphaString();
            UserGroupManager = new LocalUserGroupManager();
            UserGroupManager.CreateLocalGroup(UserGroupName);

            ContainerService = new ContainerService(ContainerBasePath, UserGroupName);
        }

        public void Dispose()
        {
            ContainerService.DestroyContainer(Container1Handle);
            ContainerService.DestroyContainer(Container2Handle);

            UserGroupManager.DeleteLocalGroup(UserGroupName);
        }

        public class Security : ContainerAcceptanceTests
        {
            [FactAdminRequired]
            public void UniqueUserPerContainer()
            {
                Container1 = CreateContainer(Container1Handle);
                Container2 = CreateContainer(Container2Handle);

                using (var c1Output = new TempFile(Container1.Directory.UserPath))
                using (var c2Output = new TempFile(Container2.Directory.UserPath))
                {
                    var pSpec = new ProcessSpec
                    {
                        ExecutablePath = "cmd.exe",
                        DisablePathMapping = true,
                        Privileged = false,
                    };

                    pSpec.Arguments = new[] { "/C", "whoami.exe", string.Format(">\"{0}\"", c1Output.FullName)};
                    Container1.Run(pSpec, null).WaitForExit();

                    pSpec.Arguments = new[] { "/C", "whoami.exe", string.Format(">\"{0}\"", c2Output.FullName) };
                    Container2.Run(pSpec, null).WaitForExit();

                    var user1 = c1Output.ReadAllText();
                    var user2 = c2Output.ReadAllText();

                    Assert.NotEmpty(user1);
                    Assert.NotEmpty(user2);
                    Assert.NotEqual(user1, user2);
                }
            }

            [FactAdminRequired]
            public void ContainerUserInContainerGroup()
            {
                Container1 = CreateContainer(Container1Handle);
                using (var c1Output = new TempFile(Container1.Directory.UserPath))
                {
                    var pSpec = new ProcessSpec
                    {
                        ExecutablePath = "cmd.exe",
                        DisablePathMapping = true,
                        Arguments = new[] { "/C", "whoami.exe", "/GROUPS", string.Format(">\"{0}\"", c1Output.FullName)},
                    };

                    Container1.Run(pSpec, null).WaitForExit();
                    var groupOutput = c1Output.ReadAllText();

                    Assert.Contains(UserGroupName, groupOutput);
                }
            }
        }

        public class Processes : ContainerAcceptanceTests
        {
            [FactAdminRequired]
            public void StartShortLivedTask()
            {
                Container1 = CreateContainer(Container1Handle);

                using (var tempFile = new TempFile(Container1.Directory.UserPath))
                {
                    var pSpec = new ProcessSpec
                    {
                        ExecutablePath = "cmd.exe",
                        DisablePathMapping = true,
                        Arguments = new string[] {"/C", string.Format("set >\"{0}\"", tempFile.FullName)},
                        Environment = new Dictionary<string, string>
                        {
                            {"PROC_ENV", "VAL1"}
                        },
                    };

                    var process = Container1.Run(pSpec, null);

                    int exitCode;
                    bool exited = process.TryWaitForExit(2000, out exitCode);

                    // VERIFY THE PROCESS RAN AND EXITED
                    Assert.True(exited);
                    Assert.Equal(0, exitCode);

                    string output = File.ReadAllText(tempFile.FullName);

                    // VERIFY THE ENVIRONMENT WAS SET
                    Assert.Contains("CONTAINER_HANDLE=" + Container1.Handle, output);
                    Assert.Contains("PROC_ENV=VAL1", output);
                }
            }
        }

        public class Properties : ContainerAcceptanceTests
        {
            ContainerSpec ContainerSpec { get; set; }

            public Properties()
            {
                ContainerSpec = new ContainerSpec
                {
                    Handle = Container1Handle,
                };
            }

            [FactAdminRequired]
            public void SetsPropertiesOnCreation()
            {
                ContainerSpec.Properties = new Dictionary<string, string>
                {
                    { "Foo", "The quick brown fox..." },
                    { "Bar", "...jumped over the lazy dog." },
                };

                Container1 = CreateContainer(ContainerSpec);

                var fooValue = Container1.GetProperty("Foo");
                var barValue = Container1.GetProperty("Bar");

                Assert.Equal("The quick brown fox...", fooValue);
                Assert.Equal("...jumped over the lazy dog.", barValue);
            }

            [FactAdminRequired]
            public void PersistsProperties()
            {
                Container1 = CreateContainer(ContainerSpec);

                Container1.SetProperty("Phrase", "The quick brown fox...");

                var value = Container1.GetProperty("Phrase");
                Assert.Equal("The quick brown fox...", value);

                Container1.RemoveProperty("Phrase");

                value = Container1.GetProperty("Phrase");
                Assert.Null(value);
            }

            [FactAdminRequired]
            public void ReturnsPropertiesInContainerInfo()
            {
                Container1 = CreateContainer(ContainerSpec);

                Container1.SetProperty("Foo", "The quick brown fox...");
                Container1.SetProperty("Bar", "...jumped over the lazy dog.");

                var info = Container1.GetInfo();

                Assert.Equal("The quick brown fox...", info.Properties["Foo"]);
                Assert.Equal("...jumped over the lazy dog.", info.Properties["Bar"]);
            }
        }

        public IContainer CreateContainer(string handle)
        {
            var bindMounts = new[]
            {
                new BindMount
                {
                    Access = FileAccess.Read,
                    SourcePath = ReadOnlyBindMountPath,
                    DestinationPath = ReadOnlyBindMountPath
                },
                new BindMount
                {
                    Access = FileAccess.ReadWrite,
                    SourcePath = ReadWriteBindMountPath,
                    DestinationPath = ReadWriteBindMountPath
                }
            };

            var environment = new Dictionary<string, string>
            {
                {"CONTAINER_HANDLE", handle},
                {"CONTAINER_ENV1", "ENV1"}
            };

            var spec = new ContainerSpec
            {
                BindMounts = bindMounts,
                Environment = environment,
                Handle = handle
            };

            return CreateContainer(spec);
        }

        public IContainer CreateContainer(ContainerSpec spec)
        {
            var container = ContainerService.CreateContainer(spec);

            return container;
        }

        private static string CreateTempDirectory()
        {
            string containerBasePath = null;
            for (var attempt = 0; attempt < 10 && string.IsNullOrWhiteSpace(containerBasePath); attempt++)
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                if (!Directory.Exists(tempPath))
                {
                    containerBasePath = tempPath;
                }
            }

            if (string.IsNullOrWhiteSpace(containerBasePath))
            {
                throw new Exception("Couldn't generate a temporary container directory");
            }

            Directory.CreateDirectory(containerBasePath);

            return containerBasePath;
        }

        private static string GenerateRandomAlphaString(int length = 8)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";

            var r = RandomFactory.Create();
            var handle = "";
            for (var count = 0; count < length; count++)
            {
                var chosenCharIndex = r.Next(0, alphabet.Length);
                handle += alphabet[chosenCharIndex];
            }

            return handle;
        }
    }

    internal class StringProcessIO : IProcessIO
    {
        public StringWriter Error = new StringWriter();
        public StringWriter Output = new StringWriter();

        public TextWriter StandardOutput
        {
            get { return Output; }
        }

        public TextWriter StandardError
        {
            get { return Error; }
        }

        public TextReader StandardInput { get; set; }
    }
}

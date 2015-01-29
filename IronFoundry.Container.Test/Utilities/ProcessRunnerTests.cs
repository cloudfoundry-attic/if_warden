using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IronFoundry.Container.Utilities
{
    public class ProcessRunnerTests
    {
        [Fact]
        public void CanRunProcess()
        {
            using (var stdinStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            using (var stdoutStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            using (var stderrStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            {
                var stdoutReader = new StreamReader(stdoutStream);
                ProcessInterop.PROCESS_INFORMATION processInformation;

                using (var stdinClientHandle = stdinStream.ClientSafePipeHandle)
                using (var stdoutClientHandle = stdoutStream.ClientSafePipeHandle)
                using (var stderrClientHandle = stderrStream.ClientSafePipeHandle)
                {
                    var startInfo = new ProcessInterop.STARTUPINFO
                    {
                        cb = Marshal.SizeOf(typeof(ProcessInterop.STARTUPINFO)),
                        dwFlags = ProcessInterop.StartInfoFlags.STARTF_USESTDHANDLES,
                        lpDesktop = "fred\\jane",
                        hStdInput = stdinClientHandle.DangerousGetHandle(),
                        hStdOutput = stdoutClientHandle.DangerousGetHandle(),
                        hStdError = stderrClientHandle.DangerousGetHandle(),
                    };

                    var commandLine = new StringBuilder();
                    commandLine.Append("ipconfig /all");

                    if (!ProcessInterop.CreateProcessWithLogonW(
                        "testuser",
                        null,
                        "testuser",
                        0U,
                        null,
                        commandLine,
                        ProcessInterop.CreationFlags.CREATE_SUSPENDED,
                        IntPtr.Zero,
                        @"C:\",
                        ref startInfo,
                        out processInformation))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                var processHandle = new ProcessSafeHandle(processInformation.hProcess);
                var threadHandle = new ThreadSafeHandle(processInformation.hThread);

                using (threadHandle)
                using (processHandle)
                {
                    Console.WriteLine("Process created with id: {0}", processInformation.dwProcessId);

                    ProcessInterop.ResumeThread(threadHandle);

                    ProcessInterop.WaitForSingleObject(processHandle, 5000);

                    int exitCode;
                    if (!ProcessInterop.GetExitCodeProcess(processHandle, out exitCode))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    Console.WriteLine("Process exited with code: {0}", exitCode);

                    var stdout = stdoutReader.ReadToEnd();
                    Console.WriteLine(stdout);
                }
            }
        }
    }

    public class ProcessSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ProcessSafeHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return ProcessInterop.CloseHandle(handle);
        }
    }

    public class ThreadSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ThreadSafeHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return ProcessInterop.CloseHandle(handle);
        }
    }

    public static class ProcessInterop
    {
        [Flags]
        public enum CreationFlags : uint
        {
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }

        [Flags]
        public enum StartInfoFlags : uint
        {
            STARTF_USESTDHANDLES = 0x00000100,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int    cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint   dwX;
            public uint   dwY;
            public uint   dwXSize;
            public uint   dwYSize;
            public uint   dwXCountChars;
            public uint   dwYCountChars;
            public uint   dwFillAttribute;
            public StartInfoFlags dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;        
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint   dwProcessId;
            public uint   dwThreadId;
        }

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public extern static bool CreateProcessWithLogonW(
          /*_In_       */ string lpUsername,
          /*_In_opt_   */ string lpDomain,
          /*_In_       */ string lpPassword,
          /*_In_       */ uint dwLogonFlags,
          /*_In_opt_   */ string lpApplicationName,
          /*_Inout_opt_*/ StringBuilder lpCommandLine,
            /*_In_       */ CreationFlags dwCreationFlags,
          /*_In_opt_   */ IntPtr lpEnvironment,
          /*_In_opt_   */ string lpCurrentDirectory,
          /*_In_       */ ref STARTUPINFO lpStartupInfo,
          /*_Out_      */ out PROCESS_INFORMATION lpProcessInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static uint WaitForSingleObject(
            SafeHandle hHandle,
            uint dwMilliseconds
        );

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(
            ProcessSafeHandle hProcess,
            out int lpExitCode
        );

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int ResumeThread(
            ThreadSafeHandle hThread
        );
    }
}

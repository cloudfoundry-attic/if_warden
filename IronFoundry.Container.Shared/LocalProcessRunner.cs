using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronFoundry.Warden.Utilities;

namespace IronFoundry.Container
{
    public interface IProcessRunner
    {
        IProcess Run(string executablePath, string[] arguments, Dictionary<string, string> environment, string workingDirectory);
    }

    public class LocalProcessRunner : IProcessRunner
    {
        readonly ProcessHelper processHelper;

        public LocalProcessRunner()
            : this(new ProcessHelper())
        {
        }

        public LocalProcessRunner(ProcessHelper processHelper)
        {
            this.processHelper = processHelper;
        }

        string EscapeArguments(string[] arguments)
        {
            // TODO: Make sure we escape each value (i.e. wrap in quotes and handle embedded quotes and backslashes).
            return String.Join(" ", arguments);
        }

        public IProcess Run(string executablePath, string[] arguments, Dictionary<string, string> environment, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = EscapeArguments(arguments),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                LoadUserProfile = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            if (environment != null && environment.Count > 0)
            {
                startInfo.EnvironmentVariables.Clear();
                foreach (var variable in environment)
                {
                    startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                }
            }

            Process p = new Process
            {
                StartInfo = startInfo,
            };

            p.EnableRaisingEvents = true;

            var wrapped = processHelper.WrapProcess(p);
            //processMonitor.TryAdd(wrapped);

            bool started = p.Start();
            Debug.Assert(started); // TODO: Should we throw an exception here? Fail fast?

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            return wrapped;
        }
    }
}

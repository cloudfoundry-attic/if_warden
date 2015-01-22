using System;
using System.Threading;
using IronFoundry.Container.Host.Handlers;
using IronFoundry.Container.Messages;
using IronFoundry.Container.Messaging;
using IronFoundry.Warden.Containers;

namespace IronFoundry.Container.Host
{
    class Program
    {
        static ManualResetEvent exitEvent = new ManualResetEvent(false);
        static ProcessTracker processTracker;

        static void Main(string[] args)
        {
            //Debugger.Launch();

            var input = Console.In;
            var output = Console.Out;
            string handle = null;

            var options = new NDesk.Options.OptionSet {
                { "handle=", v => handle = v },
            };

            options.Parse(args);

            if (String.IsNullOrWhiteSpace(handle))
                ExitWithError("Missing --handle option for starting container", 10);

            var jobObject = new JobObject(handle);
            var hostProcess = System.Diagnostics.Process.GetCurrentProcess();
            jobObject.AssignProcessToJob(hostProcess);

            using (var transport = new MessageTransport(input, output))
            {
                processTracker = new ProcessTracker(transport);

                var createProcessHandler = new CreateProcessHandler(new ProcessRunner(), processTracker);

                var dispatcher = new MessageDispatcher();
                dispatcher.RegisterMethod<CreateProcessRequest>(
                    CreateProcessRequest.MethodName,
                    async (request) =>
                    {
                        var result = await createProcessHandler.ExecuteAsync(request.@params);
                        return new CreateProcessResponse(request.id, result);
                    });

                transport.SubscribeRequest(
                    async (request) =>
                    {
                        var response = await dispatcher.DispatchAsync(request);
                        await transport.PublishResponseAsync(response);
                    });

                transport.Start();

                exitEvent.WaitOne();
            }
        }

        static void ExitWithError(string message, int exitCode)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(exitCode);
        }
    }
}

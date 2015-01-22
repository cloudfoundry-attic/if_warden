using System;
using IronFoundry.Container.Messages;
using IronFoundry.Container.Messaging;

namespace IronFoundry.Container
{
    public interface IContainerHostClient
    {
        CreateProcessResult CreateProcess(CreateProcessParams @params);
        void SubscribeToProcessData(Guid processKey, Action<ProcessDataEvent> callback);
        WaitForProcessExitResult WaitForProcessExit(WaitForProcessExitParams @params);
    }

    public class ContainerHostClient : IContainerHostClient
    {
        IMessagingClient client;

        public ContainerHostClient(IMessagingClient client)
        {
            this.client = client;
        }

        public CreateProcessResult CreateProcess(CreateProcessParams @params)
        {
            var request = new CreateProcessRequest(@params);
            var responseTask = client.SendMessageAsync<CreateProcessRequest, CreateProcessResponse>(request);
            return responseTask.GetAwaiter().GetResult().result;
        }

        public void SubscribeToProcessData(Guid processKey, Action<ProcessDataEvent> callback)
        {
            throw new NotImplementedException();
        }

        public WaitForProcessExitResult WaitForProcessExit(WaitForProcessExitParams @params)
        {
            var request = new WaitForProcessExitRequest(@params);
            var responseTask = client.SendMessageAsync<WaitForProcessExitRequest, WaitForProcessExitResponse>(request);
            return responseTask.GetAwaiter().GetResult().result;
        }
    }
}

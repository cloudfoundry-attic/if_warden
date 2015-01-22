using System;
using System.Threading.Tasks;
using IronFoundry.Container.Messages;
using IronFoundry.Container.Messaging;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace IronFoundry.Container
{
    public class ContainerHostClientTests
    {
        IMessagingClient MessagingClient { get; set; }
        ContainerHostClient Client { get; set; }

        public ContainerHostClientTests()
        {
            MessagingClient = Substitute.For<IMessagingClient>();

            Client = new ContainerHostClient(MessagingClient);
        }

        static Task<T> GetCompletedTask<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public class CreateProcess : ContainerHostClientTests
        {
            CreateProcessResult ExpectedResult { get; set; }
            CreateProcessResponse ExpectedResponse { get; set; }

            public CreateProcess()
            {
                ExpectedResult = new CreateProcessResult();
                ExpectedResponse = new CreateProcessResponse(JToken.FromObject(1), ExpectedResult);

                MessagingClient.SendMessageAsync<CreateProcessRequest, CreateProcessResponse>(null)
                    .ReturnsForAnyArgs(GetCompletedTask(ExpectedResponse));
            }

            [Fact]
            public void SendsRequestWithParams()
            {
                var @params = new CreateProcessParams
                {
                    executablePath = "foo.exe",
                };

                Client.CreateProcess(@params);

                MessagingClient.Received(1).SendMessageAsync<CreateProcessRequest, CreateProcessResponse>(
                    Arg.Is<CreateProcessRequest>(request =>
                        request.@params == @params
                    )
                );
            }

            [Fact]
            public void ReturnsResultFromResponse()
            {
                var @params = new CreateProcessParams
                {
                    executablePath = "foo.exe",
                };

                var result = Client.CreateProcess(@params);

                Assert.Same(ExpectedResult, result);
            }
        }

        public class SubscribeToProcessData : ContainerHostClientTests
        {
            
        }

        public class WaitForProcessExit : ContainerHostClientTests
        {
            WaitForProcessExitResult ExpectedResult { get; set; }
            WaitForProcessExitResponse ExpectedResponse { get; set; }

            public WaitForProcessExit()
            {
                ExpectedResult = new WaitForProcessExitResult();
                ExpectedResponse = new WaitForProcessExitResponse(JToken.FromObject(1), ExpectedResult);

                MessagingClient.SendMessageAsync<WaitForProcessExitRequest, WaitForProcessExitResponse>(null)
                    .ReturnsForAnyArgs(GetCompletedTask(ExpectedResponse));
            }

            [Fact]
            public void SendsRequestWithParams()
            {
                var @params = new WaitForProcessExitParams
                {
                    key = Guid.NewGuid(),
                    timeout = 5000,
                };

                Client.WaitForProcessExit(@params);

                MessagingClient.Received(1).SendMessageAsync<WaitForProcessExitRequest, WaitForProcessExitResponse>(
                    Arg.Is<WaitForProcessExitRequest>(request =>
                        request.@params == @params
                    )
                );
            }

            [Fact]
            public void ReturnsResultFromResponse()
            {
                var @params = new WaitForProcessExitParams
                {
                    key = Guid.NewGuid(),
                    timeout = 5000,
                };

                var result = Client.WaitForProcessExit(@params);

                Assert.Same(ExpectedResult, result);
            }
        }
    }
}

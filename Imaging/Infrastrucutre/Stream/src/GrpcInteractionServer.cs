using Visonize.UsImaging.Domain.Service.Infrastructure;
using Grpc.Core;

namespace Visonize.UsImaging.Infrastructure.Stream
{
    public class GrpcInteractionServer : IInteractionStream, IDisposable
    {
        private Thread serverThread;
        private InteractionStreamService imageStreamService;

        public GrpcInteractionServer()
        {
            serverThread = new Thread(() => RunService());
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        public event EventHandler<MouseEventData> MouseMove;
        public event EventHandler<MouseEventData> MouseDown;
        public event EventHandler<MouseEventData> MouseUp;
        public event EventHandler<string> ApiCall;

        public void Dispose()
        {
            // TODO: stop and cleanup
        }



        private void RunService()
        {
            this.imageStreamService = new InteractionStreamService();
            this.imageStreamService.MouseMove += (o, e) => this.MouseMove?.Invoke(this, e);
            this.imageStreamService.MouseDown += (o, e) => this.MouseDown?.Invoke(this, e);
            this.imageStreamService.MouseUp += (o, e) => this.MouseUp?.Invoke(this, e);
            this.imageStreamService.ApiCall += (o, e) => this.ApiCall?.Invoke(this, e);

            var server = new Grpc.Core.Server
            {
                Services = { InteractionStream.BindService(imageStreamService) },
                Ports = { new ServerPort("localhost", 50052, ServerCredentials.Insecure) }
            };

            server.Start();
            //await server.ShutdownAsync();
        }
    }

    internal class InteractionStreamService : InteractionStream.InteractionStreamBase
    {
        public event EventHandler<MouseEventData> MouseMove;
        public event EventHandler<MouseEventData> MouseDown;
        public event EventHandler<MouseEventData> MouseUp;
        public event EventHandler<string> ApiCall;

        public override async Task StreamMouseEvents(
            IAsyncStreamReader<MouseEvent> requestStream,
            IServerStreamWriter<InteractionResponse> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var mouseEvent = requestStream.Current;
                MouseEventData mouseEventData = new MouseEventData();

                mouseEventData.X = mouseEvent.X;
                mouseEventData.Y = mouseEvent.Y;

                if (mouseEvent.Button != string.Empty)
                {
                    mouseEventData.Button = Enum.Parse<MouseButton>(mouseEvent.Button);
                }

                switch(mouseEvent.EventType)
                {
                    case 1:
                        MouseMove.Invoke(this, mouseEventData);
                        break;
                    case 2:
                        MouseDown.Invoke(this, mouseEventData);
                        break;
                    case 3:
                        MouseUp.Invoke(this, mouseEventData);
                        break;
                }

                Console.WriteLine($"Mouse Event: X={mouseEvent.X}, Y={mouseEvent.Y}, Button={mouseEvent.Button}");
                await responseStream.WriteAsync(new InteractionResponse
                {
                    Status = $"Received mouse click at ({mouseEvent.X}, {mouseEvent.Y})"
                });
            }
        }

        public override async Task StreamTextMessages(
            IAsyncStreamReader<TextMessage> requestStream,
            IServerStreamWriter<InteractionResponse> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var textMsg = requestStream.Current;

                this.ApiCall.Invoke(this, textMsg.Text);

                Console.WriteLine($"Text Message: {textMsg.Text}");
                await responseStream.WriteAsync(new InteractionResponse
                {
                    Status = $"Received message: \"{textMsg.Text}\""
                });
            }
        }
    }
}


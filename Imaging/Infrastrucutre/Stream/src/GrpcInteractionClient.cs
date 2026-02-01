using Grpc.Core;
//using Grpc.Client;

namespace DupploPulse.UsImaging.Infrastructure.Stream
{
    public class GrpcInteractionClient : IDisposable
    {
        InteractionStream.InteractionStreamClient client;

        private readonly AsyncDuplexStreamingCall<MouseEvent, InteractionResponse> mouseEventsCall;
        private readonly AsyncDuplexStreamingCall<TextMessage, InteractionResponse> messageCall;


        public GrpcInteractionClient()
        {
            var channel = new Channel("localhost:50052", ChannelCredentials.Insecure);
            this.client = new InteractionStream.InteractionStreamClient(channel);
            this.mouseEventsCall = client.StreamMouseEvents();
            this.messageCall = client.StreamTextMessages();
        }

        public void SendMouseEvents(MouseEvent mouseEvent)
        {
            //using var call = client.StreamMouseEvents();

            //var responseReader = Task.Run(async () =>
            //{
            //    await foreach (var resp in call.ResponseStream.ReadAllAsync())
            //    {
            //        Console.WriteLine($"[Server->MouseStream] {resp.Status}");
            //    }
            //});

            //for (int i = 0; i < 3; i++)
            //{
            //    await call.RequestStream.WriteAsync(new MouseEvent { X = i * 10, Y = i * 15, Button = "left" });
            //    await Task.Delay(500);
            //}
            try
            {
                this.mouseEventsCall.RequestStream.WriteAsync(mouseEvent).Wait();
            }
            catch
            { }
            //await call.RequestStream.CompleteAsync();
            //await responseReader;
        }

        public void SendTextMessages(string message)
        {
            var textMessage = new TextMessage();
            textMessage.Text = message;



                this.messageCall.RequestStream.WriteAsync(textMessage);




            //using var call = client.StreamTextMessages();

            //var responseReader = Task.Run(async () =>
            //{
            //    await foreach (var resp in call.ResponseStream.ReadAllAsync())
            //    {
            //        Console.WriteLine($"[Server->TextStream] {resp.Status}");
            //    }
            //});

            //var messages = new[] { "Hello", "How are you?", "Goodbye" };
            //foreach (var msg in messages)
            //{
            //    await call.RequestStream.WriteAsync(new TextMessage { Text = msg });
            //    await Task.Delay(800);
            //}

            //await call.RequestStream.CompleteAsync();
            //await responseReader;
        }

        public void Dispose()
        {
            this.mouseEventsCall.Dispose();
        }
    }
}

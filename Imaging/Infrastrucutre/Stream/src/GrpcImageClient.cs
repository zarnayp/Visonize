using System.Diagnostics;
using Grpc.Core;

namespace Visonize.UsImaging.Infrastructure.Stream
{
    public class GrpcImageClient : IDisposable
    {
        private AsyncServerStreamingCall<ImageReply> call;

        public event EventHandler<byte[]> FrameReceived;

        public GrpcImageClient()
        {
            var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
            var client = new ImageStreamer.ImageStreamerClient(channel);

            var request = new ImageRequest { Filter = "" }; // can be left empty

            Task.Run(() => ListenerThread());

            this.call = client.StreamImages(request);
        }

        public void Dispose()
        {
            // TODO: stop thread and cleanup

            this.call.Dispose();
        }


        private async Task ListenerThread()
        {
            SpinWait spinWait = new SpinWait();
            while (call == null)
            {
                spinWait.SpinOnce();
            }

            int count = 0;
            Debug.WriteLine("kk");
            while (await call.ResponseStream.MoveNext())
            {
                var image = call.ResponseStream.Current.ImageData.ToByteArray();

                FrameReceived?.Invoke(this, image);
            }

        }
        // Optional: save to disk or process image
        // File.WriteAllBytes($"received_{Guid.NewGuid()}.jpg", imageBytes);


    }
}

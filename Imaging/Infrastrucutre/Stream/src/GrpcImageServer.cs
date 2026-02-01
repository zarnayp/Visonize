using System.Diagnostics;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Infrastructure.Common;
using Grpc.Core;

namespace DupploPulse.UsImaging.Infrastructure.Stream
{
    public class GrpcImageServer : IImageStream, IDisposable
    {
        private Thread serverThread;
        private ImageStreamerService imageStreamService;

        public GrpcImageServer()
        {
            serverThread = new Thread(() => RunService());
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        public void Dispose()
        {
            // TODO: stop and cleanup
        }

        public void UpdateImage(IRgbImageReference image)
        {
            this.imageStreamService?.UpdateImage(image);
        }

        private void RunService()
        {
            this.imageStreamService = new ImageStreamerService();
            var server = new Grpc.Core.Server
            {
                Services = { ImageStreamer.BindService(imageStreamService) },
                Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) }
            };

            server.Start();
            //await server.ShutdownAsync();
        }
    }


    public class ImageStreamerService : ImageStreamer.ImageStreamerBase
    {
        private object _lock = new object();
        private AutoResetEvent imageUpdateEvent = new AutoResetEvent(false);
        private IRgbImageReference image;

        public override async Task StreamImages(ImageRequest request, IServerStreamWriter<ImageReply> responseStream, ServerCallContext context)
        {
            Thread.Sleep(1000);

            while (true)
            {
                imageUpdateEvent.WaitOne();

                Debug.WriteLine("Wait done.");
                var sw = Stopwatch.StartNew();
                long ms = 0;
                Google.Protobuf.ByteString imageData;
                lock (_lock)
                {
                    var imageBuffer = ((ManagedHeapRGBImageReference)this.image).Rgb;
                    imageData = Google.Protobuf.ByteString.CopyFrom(imageBuffer);
                    ms = sw.ElapsedMilliseconds;
                }
                try
                {
                    await responseStream.WriteAsync(new ImageReply
                    {
                        ImageData = imageData
                    });
                }
                catch(Exception e)
                {
                    Debug.WriteLine($"Client finished");
                    break;
                }
                Debug.WriteLine($"Pushed {ms} , {sw.ElapsedMilliseconds}");
            }
        }

        public void UpdateImage(IRgbImageReference image)
        {
            lock (_lock)
            {
                this.image = image;
            }
            imageUpdateEvent.Set();
        }
    }


}

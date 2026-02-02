using System.Diagnostics;
using Visonize.UsImaging.Infrastructure.Common;

namespace Visonize.UsImaging.Infrastructure.Stream.Tests
{
    public class GrpcServerClientTests
    {
        [Test]
        public void GrpcServer_TestUpdate()
        {
            using (var server = new Visonize.UsImaging.Infrastructure.Stream.GrpcImageServer())
            {
                Thread.Sleep(1000);

                using (var client = new Visonize.UsImaging.Infrastructure.Stream.GrpcImageClient())
                {
                    var iamge = new ManagedHeapRGBImageReference(new byte[1024*720*4]);

                    byte[]? bufferRecieved = null;
                    int i = 0;

                    client.FrameReceived += (sender, e) => 
                    {
                        i++;
                        bufferRecieved = e;
                    };

                    server.UpdateImage(iamge);
                    Thread.Sleep(16);
                    server.UpdateImage(iamge);
                    Thread.Sleep(16);
                    server.UpdateImage(iamge);
                    Thread.Sleep(16);
                    server.UpdateImage(iamge);

                    var sw = Stopwatch.StartNew();
                    var spin = new SpinWait();
                    while(bufferRecieved == null || sw.ElapsedMilliseconds<1000)
                    {
                        spin.SpinOnce();
                    }

                    Assert.IsNotNull(bufferRecieved);

                    Assert.AreEqual(2, i);
                }
            }
        }
    }
}

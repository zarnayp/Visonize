namespace Visonize.UsImaging.Infrastructure.Stream.Tests;

public class WebRTCImageStreamerRecieverTest
{
    [Test]
    public void WebRTCImageStreamerRecieverTest_ExtractWithEightThreads()
    {
        WebRTCImageStreamer streamer = new WebRTCImageStreamer();
        
        byte[] image = new byte[100];

        streamer.SendImage(image);
    }
}
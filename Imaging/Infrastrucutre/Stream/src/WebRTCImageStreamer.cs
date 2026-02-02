using System.Threading.Tasks;
using SIPSorcery.Net;

namespace Visonize.UsImaging.Infrastructure.Stream;

public class WebRTCImageStreamer
{
    private RTCPeerConnection _peerConnection;
    private RTCDataChannel _dataChannel;

    public WebRTCImageStreamer()
    {
        // Initialize WebRTC Peer Connection
        var configuration = new RTCConfiguration();
        
        _peerConnection = new RTCPeerConnection(configuration);

        // Create a DataChannel for sending images
        _dataChannel = _peerConnection.createDataChannel("imageStream", new RTCDataChannelInit { ordered = true }).Result;

        _dataChannel.onopen += () => Console.WriteLine("DataChannel Opened.");
        _dataChannel.onclose += () => Console.WriteLine("DataChannel Closed.");
        _dataChannel.onerror += (error) => Console.WriteLine($"DataChannel Error: {error}");

        Console.WriteLine("WebRTC Server Initialized.");
    }

    public async Task SendImage(byte[] jpegXs)
    {
        if (_dataChannel == null || _dataChannel.readyState != RTCDataChannelState.open)
        {
            Console.WriteLine("DataChannel is not ready.");
            return;
        }

        // Send JPEG XS frame over WebRTC DataChannel
        _dataChannel.send(jpegXs);
        Console.WriteLine("JPEG XS frame sent.");
    }
}
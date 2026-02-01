using System;
using System.Threading.Tasks;
using SIPSorcery.Net;

public class WebRTCImageReceiver
{
    private RTCPeerConnection _peerConnection;
    private RTCDataChannel _dataChannel;

    public WebRTCImageReceiver()
    {
        // Initialize WebRTC Peer Connection
        _peerConnection = new RTCPeerConnection(null);

        // Handle incoming DataChannel
        _peerConnection.ondatachannel += (dc) =>
        {
            _dataChannel = dc;
            _dataChannel.onmessage += (channel, messageEvent, message) =>
            {
                byte[] receivedData = message;
                ProcessReceivedImage(receivedData);
            };

            Console.WriteLine("DataChannel connected.");
        };

        Console.WriteLine("WebRTC Client Initialized.");
    }

    private void ProcessReceivedImage(byte[] jpegXsData)
    {
        Console.WriteLine($"Received JPEG XS frame: {jpegXsData.Length} bytes");
        // Process or display the image as needed
    }
}
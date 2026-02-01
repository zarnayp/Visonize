using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastracture.Beamformer
{
    public class BeamformerAdapter : IBeamformer
    {
        ISystemTime systemTime;

        public BeamformerAdapter()
        {
            this.systemTime = new SystemTime();
        }

        public RawFrameDescription Process(RFFrameDescription rFFrameDescription, IRawFrameReference targetRawFrameReference)
        {
            var sourceImageBuffer = ((ManagedMemoryFrameReference)rFFrameDescription.FrameReference).FrameBuffer;
            var destinationImage = (ManagedMemoryFrameReference)targetRawFrameReference;

            Buffer.BlockCopy(sourceImageBuffer, 0, destinationImage.FrameBuffer, 0, sourceImageBuffer.Length);


            var rawFrameMetaData = new RawFrameMetaData();
            rawFrameMetaData.AcquisitionTimeStamp = this.systemTime.GetMicrosecondTimestamp();  // TODO: should be taken from RFFramedescription

            var frameDescription = new RawFrameDescription(destinationImage, rawFrameMetaData);

            return frameDescription;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastracture.ImageSource
{
    public class DicomFileCine : IArchivedDataSource
    {
        private readonly int numberOfFrames;
        private readonly byte[][] framesData;
        private readonly RawFrameMetaData[] rawFrameMetaData;

        public DicomFileCine(int numberOfFrames)
        {
            this.numberOfFrames = numberOfFrames;
            this.framesData = new byte[numberOfFrames][];
            this.rawFrameMetaData = new RawFrameMetaData[numberOfFrames];
        }

        //public void Set
        public void SetFrame(int frameIndex, byte[] imageData, RawFrameMetaData metaData)
        {
            if (frameIndex < 0 || frameIndex >= numberOfFrames)
            {
                // TODO: log error
                return;
            }

            framesData[frameIndex] = imageData;
            rawFrameMetaData[frameIndex] = metaData;
        }

        public RawFrameDescription GetCurrentFrame()
        {
            var rawFrameReference = new ManagedMemoryFrameReference(this.framesData[0]);
            var metaData = this.rawFrameMetaData[0];

            var frameDesciption = new RawFrameDescription(rawFrameReference, metaData);

            return frameDesciption;
        }

        public void ScrollByFrame(int numOfFrames)
        {
            throw new NotImplementedException();
        }

        public void ScrollByTime(int microseconds)
        {
            throw new NotImplementedException();
        }


    }
}

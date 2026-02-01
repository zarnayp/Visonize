
using System;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Infrastracture.Player;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastracture.ImageSource
{
    public class Cine : ICine
    {
        private int currentFrame = -1;
        private uint numOfFrames;
        private bool isLiveMode = false;

        private Dictionary<int, Tuple<ManagedMemoryFrameReference, RawFrameMetaData?>> frames = new ();
        private List<byte[]> buffer;
        private uint frameImageSize;

        public Cine(uint numberOfFrames)
        {
            frameImageSize = 1023 * 720;

            buffer = new List<byte[]>();
            for (int i=0; i<numberOfFrames; i++)
            {
                buffer.Add(new byte[frameImageSize]);
            }
            this.numOfFrames = numberOfFrames;
            Flush();
        }


        public IRawFrameReference GetTargetFrameBuffer()
        {
            if (!this.isLiveMode)
            {
                this.Flush();
                this.isLiveMode = true;
            }
            else
            {
                if (this.frames[currentFrame].Item2 == null)
                {
                    throw new InvalidOperationException("Previous frame not completed.");
                }
            }

            currentFrame = GetNextFrameId(currentFrame);

            return frames[currentFrame].Item1;
        }

        public void PushedTargetFrame(RawFrameMetaData metaData)
        {
            this.frames[currentFrame] = new (this.frames[currentFrame].Item1, metaData);
        }

        public int NumberOfImages
        {
            get
            {
                return frames.Count;
            }
        }

        private int GetNextFrameId(int frameId)
        {
            var result = frameId + 1 == numOfFrames ? 0 : frameId + 1;
            return result;
        }

        private void Flush()
        {
            this.currentFrame = -1;

            foreach (var frame in this.frames)
            {
                frame.Value.Item1.Dispose();
            }

            this.frames = new ();
            for (int i = 0; i < numOfFrames; i++)
            {
                this.frames[i] = new (new ManagedMemoryFrameReference(this.buffer[i]), null);
            }
        }

        public RawFrameDescription GetCurrentFrame()
        {
            return new (this.frames[this.currentFrame].Item1, (RawFrameMetaData)this.frames[this.currentFrame].Item2);
        }

        public void ScrollByTime(int moveInMicroseconds)
        {
            this.isLiveMode = false;

            throw new NotImplementedException();
        }

        public void ScrollByFrame(int delta)
        {
            this.isLiveMode = false;

            // Add delta and wrap
            int newIndex = (int)(this.currentFrame + delta) % (int)this.numOfFrames;

            // Ensure positive result for negative
            if (newIndex < 0)
            {
                newIndex += (int)this.numOfFrames;
            }

            this.currentFrame = newIndex;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;

namespace DupploPulse.UsImaging.Infrastructure.Common
{

    public class ManagedMemoryFrameReference : IRawFrameReference, IRFFrameReference, IDisposable
    {
        private static byte[] removedFrameBuffer = new Byte[1024*720*3];
        private byte[] frameBuffer;
        private int frameBufferSize;
        private bool isDisposed = false;

        public ManagedMemoryFrameReference(byte[] frameBuffer)
        {
            this.frameBuffer = frameBuffer;
        }

        public byte[] FrameBuffer
        {
            get
            {
                if(this.isDisposed)
                {
                    if(removedFrameBuffer.Length < this.frameBufferSize)
                    {
                        removedFrameBuffer = new byte[this.frameBufferSize];
                    }
                    return removedFrameBuffer;
                }
                return frameBuffer;
            }
        }

        public void Dispose()
        {
            this.frameBufferSize = this.frameBuffer.Length;
            this.frameBuffer = null;
        }
    }
}

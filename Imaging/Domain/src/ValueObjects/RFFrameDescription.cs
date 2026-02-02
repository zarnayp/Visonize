using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Visonize.UsImaging.Domain.Infrastracture.Common;

namespace Visonize.UsImaging.Domain.ValueObjects
{
    public struct RFFrameDescription
    {
        public RFFrameDescription(IRFFrameReference frame)
        {
            this.FrameReference = frame;
        }

        int NumberOfLines { get; }


        public IRFFrameReference FrameReference { get; }
    }
}
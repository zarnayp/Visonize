using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.SceneObjects;

namespace DupploPulse.UsImaging.Domain.ValueObjects
{
    public ref struct RawFrameDescription
    {
        public RawFrameDescription(IRawFrameReference frameReference, RawFrameMetaData metaData)
        {
            this.RawFrameReference = frameReference;
            this.RawFrameMetaData = metaData;
        }

        public IRawFrameReference RawFrameReference { get; }

        public RawFrameMetaData RawFrameMetaData { get; }
    }

    public struct RawFrameMetaData()
    {
        public long AcquisitionTimeStamp;

        public IGeometryData GeometryData;
    }
}

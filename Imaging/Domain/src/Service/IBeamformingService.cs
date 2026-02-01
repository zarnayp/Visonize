using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.ValueObjects;

namespace DupploPulse.UsImaging.Domain.Service
{
    public interface IBeamformingService
    {
        RawFrameDescription ProcessRFData(RFFrameDescription rFFrameDescription, IRawFrameReference rawFrameReference);
    }
}

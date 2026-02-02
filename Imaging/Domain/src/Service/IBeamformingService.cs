using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.ValueObjects;

namespace Visonize.UsImaging.Domain.Service
{
    public interface IBeamformingService
    {
        RawFrameDescription ProcessRFData(RFFrameDescription rFFrameDescription, IRawFrameReference rawFrameReference);
    }
}

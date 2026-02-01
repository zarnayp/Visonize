using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.ValueObjects;

namespace DupploPulse.UsImaging.Domain.Player
{
    public interface IBeamformer
    {
        RawFrameDescription Process(RFFrameDescription description, IRawFrameReference rawFrameReference);
    }
}
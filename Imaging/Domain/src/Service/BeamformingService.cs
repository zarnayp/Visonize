using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.ValueObjects;

namespace DupploPulse.UsImaging.Domain.Service
{
    public class BeamformingService : IBeamformingService
    {
        private IBeamformer beamformer;

        public BeamformingService(IBeamformer beamformer)
        {
            this.beamformer = beamformer;
        }

        public RawFrameDescription ProcessRFData(RFFrameDescription rFFrameDescription, IRawFrameReference rawFrameReference)
        {
            return this.beamformer.Process(rFFrameDescription, rawFrameReference);
        }
    }
}

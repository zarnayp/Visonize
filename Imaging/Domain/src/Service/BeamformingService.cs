using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Player;
using Visonize.UsImaging.Domain.ValueObjects;

namespace Visonize.UsImaging.Domain.Service
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

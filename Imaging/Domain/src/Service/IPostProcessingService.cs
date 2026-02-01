using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Domain.Entities.Scene;

namespace DupploPulse.UsImaging.Domain.Service
{
    public interface IPostProcessingService
    {
        //void ProcessRawData(RawFrameDescription rawFrameDescription, IRgbImageReference rgbImageReference);

        void Process(IRgbImageReference targetImageReference);

        ImageViewArea ImageViewArea {  get; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Visonize.UsImaging.Domain.ValueObjects;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.Entities.Scene;

namespace Visonize.UsImaging.Domain.Service
{
    public interface IPostProcessingService
    {
        //void ProcessRawData(RawFrameDescription rawFrameDescription, IRgbImageReference rgbImageReference);

        void Process(IRgbImageReference targetImageReference);

        ImageViewArea ImageViewArea {  get; }

    }
}

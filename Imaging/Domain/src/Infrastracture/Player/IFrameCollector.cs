using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;

namespace DupploPulse.UsImaging.Domain.Infrastracture.Player
{
    public interface IRgbFrameCollector
    {
        IRgbImageReference GetTargetImage();
    }
}

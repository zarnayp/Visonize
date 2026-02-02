using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Service.Infrastructure;

namespace Visonize.UsImaging.Domain.Infrastracture.Player
{
    public interface IRgbFrameCollector
    {
        IRgbImageReference GetTargetImage();
    }
}

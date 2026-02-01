using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Player;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastracture.ImageSource
{
    public class RgbImageCollector : IRgbFrameCollector
    {
        private IRgbImageReference tmpRgbImage = new ManagedHeapRGBImageReference(new byte[1024 * 720 * 4]);

        public IRgbImageReference GetTargetImage()
        {
            return tmpRgbImage;
        }
    }
}

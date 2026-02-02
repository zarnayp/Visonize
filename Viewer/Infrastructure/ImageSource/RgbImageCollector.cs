using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Player;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Infrastructure.Common;

namespace Visonize.UsImaging.Infrastracture.ImageSource
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

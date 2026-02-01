using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.SceneObjects;

namespace DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects
{
    public interface ITwoDImageSceneObject : IRenderableTwoDImageSceneObject
    {
        void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.SceneObjects;
using PragmaticScene.RenderableInterfaces;

namespace DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects
{
    public interface IThreeDImageSceneObject : IRenderableSceneObject
    {
        void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry);

    }
}

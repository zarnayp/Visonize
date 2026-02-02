using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.SceneObjects;
using PragmaticScene.RenderableInterfaces;

namespace Visonize.UsImaging.Domain.Infrastracture.SceneObjects
{
    public interface IThreeDImageSceneObject : IRenderableSceneObject
    {
        void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry);

    }
}

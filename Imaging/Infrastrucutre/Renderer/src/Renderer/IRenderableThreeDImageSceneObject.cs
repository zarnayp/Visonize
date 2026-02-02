using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.SceneObjects;
using PragmaticScene.RenderableInterfaces;

namespace Visonize.UsImaging.Infrastructure.Renderer.Renderer
{
    internal interface IRenderableThreeDImageSceneObject : IRenderableSceneObject
    {
        IRawFrameReference? ImageData { get; }

        IGeometryData? GeometryData { get; }
    }
}

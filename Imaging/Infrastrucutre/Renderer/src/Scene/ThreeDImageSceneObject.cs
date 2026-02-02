using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Infrastracture.SceneObjects;
using Visonize.UsImaging.Domain.SceneObjects;
using Visonize.UsImaging.Infrastructure.Renderer.Renderer;

namespace Visonize.UsImaging.Infrastructure.Renderer.Scene
{
    internal class ThreeDImageSceneObject : IThreeDImageSceneObject, IRenderableThreeDImageSceneObject
    {
        private IRawFrameReference? imageData = null;

        private IGeometryData? geometryData = null;

        private Matrix4x4 transformation = Matrix4x4.Identity;

        public bool IsVisible => true;

        public Matrix4x4 Transformation => this.transformation;

        public IRawFrameReference? ImageData { get => this.imageData; }

        public IGeometryData? GeometryData { get => this.geometryData; }

        public void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry)
        {
            this.imageData = imageData;
            this.geometryData = geometry;
        }
    }
}

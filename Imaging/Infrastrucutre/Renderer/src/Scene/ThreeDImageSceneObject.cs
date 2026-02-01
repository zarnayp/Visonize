using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.Infrastructure.Renderer.Renderer;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Scene
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

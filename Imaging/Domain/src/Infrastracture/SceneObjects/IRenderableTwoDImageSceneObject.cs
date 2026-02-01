using PragmaticScene.RenderableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;

namespace DupploPulse.UsImaging.Domain.SceneObjects
{
    public interface IGeometryData
    {
    }

    public struct CurvedGeometryData : IGeometryData
    {
        public float AxialMinMm;
        public float AxialSpanMm;
        public float AzimMinRad;
        public float AzimSpanRad;
        public float AzimApexZPosMm;
    }

    public struct VolumeCartezianGeometry : IGeometryData
    {
        public float Width;
        public float Height; 
        public float Depth;
        public float MmPerPixel;
    }

    public struct ScanConversionAtributes
    {
        public ScanConversionAtributes()
        {
        }

        public float MmPerPixel = 0.25f;
        public float CenterX = 0.5f;
        public float CenterY = 0.0f;
    }

    public interface IRenderableTwoDImageSceneObject : IRenderableSceneObject
    {
        IRawFrameReference? ImageData { get; }

        IGeometryData? GeometryData { get; }

        ScanConversionAtributes ScanConversionAtributes { get; }

        byte[]? Lut {  get; }

    }
}

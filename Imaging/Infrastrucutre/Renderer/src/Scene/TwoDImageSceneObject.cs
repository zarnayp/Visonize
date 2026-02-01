using PragmaticScene.RenderableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using System.Drawing;
using DupploPulse.UsImaging.Domain.Entities.Algs;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Scene
{
    internal class TwoDImageSceneObject : ITwoDImageSceneObject
    {
        private IRawFrameReference? imageData;

        private IGeometryData? geometryData;

        private ScanConversionAtributes scanConversionAtributes = new ScanConversionAtributes();

        private byte[] lut = new byte[256*4];

        private int tintIndex = 0;

        private int grayMapIndex = 3;


        public bool IsVisible => true;

        public Matrix4x4 Transformation => Matrix4x4.Identity;

        public IRawFrameReference? ImageData => imageData;

        public IGeometryData? GeometryData => geometryData;

        public ScanConversionAtributes ScanConversionAtributes
        {
            get => scanConversionAtributes;
            set => scanConversionAtributes = value;
        }

        public byte[]? Lut => this.lut;

        public TwoDImageSceneObject()
        {
            this.lut = ImageLut.GetLut(this.tintIndex, this.grayMapIndex);
        }

        public void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry)
        {
            this.imageData = imageData;
            this.geometryData = geometry;
        }

        public int TintIndex
        {
            get
            {
                return this.tintIndex;
            }
            set
            {
                if (this.tintIndex != value)
                {
                    this.tintIndex = value;
                    this.lut = ImageLut.GetLut(this.tintIndex, this.grayMapIndex);
                }
            }
        }

        public int GrayMap
        {
            get
            {
                return this.grayMapIndex;
            }
            set
            {
                if (this.grayMapIndex != value)
                {
                    this.grayMapIndex = value;
                    this.lut = ImageLut.GetLut(this.tintIndex, this.grayMapIndex);
                }
            }
        }
    }
}

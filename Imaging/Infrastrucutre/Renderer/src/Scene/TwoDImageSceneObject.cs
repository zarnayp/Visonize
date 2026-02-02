using PragmaticScene.RenderableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using System.Drawing;
using Visonize.UsImaging.Domain.Entities.Algs;
using Visonize.UsImaging.Domain.SceneObjects;
using Visonize.UsImaging.Domain.Infrastracture.SceneObjects;

namespace Visonize.UsImaging.Infrastructure.Renderer.Scene
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

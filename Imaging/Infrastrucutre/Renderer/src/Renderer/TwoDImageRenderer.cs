using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.Runtime.InteropServices;
using Diligent;
using PragmaticScene.RendererInterfaces;
using PragmaticScene.RenderableInterfaces;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.ImagePostProcessing.ImageProcessors;
using DupploPulse.UsImaging.Infrastructure.Common;

using System.Numerics;

namespace DupploPulse.UsImaging.Infrastructure.Renderer
{
    internal class TwoDImageRenderer : IProjectedSceneObjectRenderer
    {
        private readonly IRenderableTwoDImageSceneObject twoDImage;
        private readonly ScanConverter scanConverter;
        private readonly ColorFilters colorFilters;
        private readonly Diligent.IDeviceContext deviceContext;
        private readonly IRenderDevice rendererDevice;
        private ITextureView texture;
        private readonly uint TextureWidth = 1024;
        private readonly uint TextureHeight = 512;
        private readonly byte[] ClearImage = new byte[1024 * 512];

        private TextureDesc stagingTextureDesc;

        private ITextureView dsvView;

        private ITexture scanConvertedImageTexture;
        private ITextureView scanConvertedImageTextureRenderTargetView;
        private ITextureView scanConvertedImageTextureShaderResourceView;

        private byte[] currentLut;
        private Viewport previousViewport = new Viewport();

        public TwoDImageRenderer(IRenderDevice rendererDevice, Diligent.IDeviceContext deviceContext, IEngineFactory engineFactory, ISwapChain swapChain, IRenderableTwoDImageSceneObject twoDImage)
        {


            this.rendererDevice = rendererDevice;
            this.deviceContext = deviceContext;

            var depthTexDesc = new TextureDesc
            {
                Type = ResourceDimension.Tex2d,
                Width = 1024,
                Height = 720,
                Format = TextureFormat.D32_Float,
                BindFlags = BindFlags.DepthStencil,
            };
            var depthTex = rendererDevice.CreateTexture(depthTexDesc, null);
            var dsvView = depthTex.GetDefaultView(TextureViewType.DepthStencil);



            this.twoDImage = twoDImage;

            texture = LoadVolume();

            this.scanConverter = new ScanConverter(rendererDevice, deviceContext, engineFactory, swapChain, texture);
            this.colorFilters= new ColorFilters(rendererDevice, deviceContext, engineFactory, swapChain);
        }

        public ITextureView LoadVolume()
        {
            var descritpion = new TextureDesc
            {
#if LINUX
                ImmediateContextMask = 1,
#endif
                Type = ResourceDimension.Tex2d,
                Name = "Volume",
                Height = TextureHeight,
                Width = TextureWidth,
                Format = TextureFormat.R8_UNorm,
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.Write
            };

            var texture = rendererDevice.CreateTexture(descritpion);

            return texture.GetDefaultView(TextureViewType.ShaderResource);
        }

        public IRenderableSceneObject SceneObject => twoDImage;

        public void Render(ITextureView rtv, ITextureView dsv, in ViewProjectionData viewProjectionData)
        {
            if (this.previousViewport.Width != viewProjectionData.Viewport.Width ||
                this.previousViewport.Height != viewProjectionData.Viewport.Height)
            {
                UpdateScanConvertedImageTexture(viewProjectionData.Viewport);
                this.previousViewport = viewProjectionData.Viewport;
            }

            var imageReference = twoDImage.ImageData as ManagedMemoryFrameReference;

            var mappedTexture = deviceContext.MapTextureSubresource(texture.GetTexture(), 0, 0, MapType.Write, MapFlags.Discard, null);

            if (imageReference == null)
            {
                Marshal.Copy(ClearImage, 0, mappedTexture.Data, (int)(TextureWidth * TextureHeight));
            }
            else
            {
                for (int i = 0; i < 179; i++)
                {
                    Marshal.Copy(imageReference.FrameBuffer, i * 848, mappedTexture.Data + i * (int)(ulong)mappedTexture.Stride, 848);
                }
            }

            deviceContext.UnmapTextureSubresource(texture.GetTexture(), 0, 0);

            var textureViewDesc = new TextureViewDesc();
            textureViewDesc.ViewType = TextureViewType.RenderTarget;
            textureViewDesc.AccessFlags = UavAccessFlag.FlagReadWrite;
            textureViewDesc.Flags = TextureViewFlags.None;

            deviceContext.SetRenderTargets(new[] { this.scanConvertedImageTextureRenderTargetView }, this.dsvView, ResourceStateTransitionMode.Transition);

            scanConverter.Render(viewProjectionData.Viewport, viewProjectionData.ViewMatrix, viewProjectionData.ProjectionMatrix);

            // Image filters
            if (this.currentLut != twoDImage.Lut && twoDImage.Lut != null)
            {
                this.currentLut = this.twoDImage.Lut;
                this.colorFilters.UpdateLut(twoDImage.Lut);
            }

            deviceContext.SetRenderTargets(new[] { rtv }, null, ResourceStateTransitionMode.Transition);
            colorFilters.Render(scanConvertedImageTextureShaderResourceView, viewProjectionData.Viewport);
        }

        private void UpdateScanConvertedImageTexture(Viewport viewport)
        {
            if(this.scanConvertedImageTexture !=null)
            {
                this.scanConvertedImageTexture.Dispose();

                // TODO: check if Dispose is not needed for views
                //this.scanConvertedImageTextureRenderTargetView.Dispose();
                //this.scanConvertedImageTextureShaderResourceView.Dispose();
            }

            this.stagingTextureDesc = new TextureDesc()
            {
                Type = ResourceDimension.Tex2d,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = Usage.Default,
                Format = TextureFormat.RGBA8_UNorm,
                Height = (uint)viewport.Height,
                Width = (uint)viewport.Width,
                MipLevels = 1,
                ArraySizeOrDepth = 1,
                CPUAccessFlags = CpuAccessFlags.None,

            };

            this.scanConvertedImageTexture = this.rendererDevice.CreateTexture(stagingTextureDesc);
            this.scanConvertedImageTextureRenderTargetView = this.scanConvertedImageTexture.GetDefaultView(TextureViewType.RenderTarget);
            this.scanConvertedImageTextureShaderResourceView = this.scanConvertedImageTexture.GetDefaultView(TextureViewType.ShaderResource);

        }
    }
}

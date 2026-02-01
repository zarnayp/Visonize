using Diligent;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DupploPulse.UsImaging.ImagePostProcessing.ImageProcessors
{
    internal class ColorFilters : IDisposable
    {
        private readonly IRenderDevice renderDevice;
        private readonly IDeviceContext deviceContext;

        private readonly IBuffer uniformBuffer;
        private readonly ITextureView inputTexture;
        private ITexture lutTexture;
        private ITextureView lutTextureView;
        private IShaderSourceInputStreamFactory shaderSourceFactory;

        private IPipelineState pipeline;
        private IShaderResourceBinding shaderResourceBinding;

        private byte[]? lut = null;
        private byte[] defaultLut;

        struct ScanConverterDesc
        {
            public float MmPerPixel;
            public float CenterX;
            public float CenterY;
        }

        public float MmPerPixel
        {
            get; set;
        }

        public float CenterX
        {
            get; set;
        }

        public float CenterY
        {
            get; set;
        }

        public void UpdateLut(byte[] lut)
        {
            var mappedData = this.deviceContext.MapTextureSubresource(
                this.lutTexture,
                mipLevel: 0,
                arraySlice: 0,
                MapType.Write,
                MapFlags.Discard,
                null
            );

            IntPtr rowPtr = mappedData.Data;
            Marshal.Copy(lut, 0, mappedData.Data, lut.Length);

            // Unmap texture
            deviceContext.UnmapTextureSubresource(this.lutTexture, mipLevel: 0, arraySlice: 0);
        }

        public ColorFilters(IRenderDevice renderDevice, Diligent.IDeviceContext deviceContext, IEngineFactory engineFactory, ISwapChain swapChain)
        {
            this.defaultLut = new byte[256 * 4];

            for (int i = 0; i < 256; i++)
            {
                byte r = (byte)i;
                byte g = (byte)i;
                byte b = (byte)i;
                byte a = 255;

                defaultLut[i * 4 + 0] = r;
                defaultLut[i * 4 + 1] = g;
                defaultLut[i * 4 + 2] = b;
                defaultLut[i * 4 + 3] = a;
            }

            this.renderDevice = renderDevice;
            this.deviceContext = deviceContext;

            this.uniformBuffer = renderDevice.CreateBuffer(new()
            {
#if LINUX
                ImmediateContextMask = 1,
#endif
                Name = "Uniform buffer",
                Size = new SharpGen.Runtime.NativeULong((ulong)Unsafe.SizeOf<ScanConverterDesc>()),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            this.shaderSourceFactory = engineFactory.CreateDefaultShaderSourceStreamFactory("UltrasoundShaders");

            CreatePipeline();
        }

        private DateTime lastShaderDateTime;

        private void CreatePipeline()
        {
            DateTime currentDateTime = File.GetLastWriteTime("UltrasoundShaders//colorFiltersPS.psh");
            if (currentDateTime == lastShaderDateTime)
            {
                return;
            }
            lastShaderDateTime = currentDateTime;

            var vs = renderDevice.CreateShader(new()
            {
                FilePath = "colorFiltersVS.vsh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "colorFilters VS",
                    ShaderType = ShaderType.Vertex,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);

            if (vs == null)
            {
                return;
            }

            var ps = renderDevice.CreateShader(new()
            {
                FilePath = "colorFiltersPS.psh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "colorFilters PS",
                    ShaderType = ShaderType.Pixel,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);

            if (ps == null)
            {
                return;
            }

            this.pipeline = renderDevice.CreateGraphicsPipelineState(new()
            {
                PSODesc = new()
                {
                    Name = "Color Filters PSO",
                    ResourceLayout = new()
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[] {
                            new ()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "g_Input",
                                Type = ShaderResourceVariableType.Dynamic,

                            },
                            new ()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "g_LUT",
                                Type = ShaderResourceVariableType.Mutable,

                            }
                        }
                    }
                },
                Vs = vs,
                Ps = ps,

                GraphicsPipeline = new()
                {
                    SmplDesc = new()
                    {
                        Count = 1,
                        Quality = 0
                    },

                    InputLayout = new InputLayoutDesc { LayoutElements = Array.Empty<LayoutElement>() },
                    PrimitiveTopology = PrimitiveTopology.TriangleList,

                    RasterizerDesc = new()
                    {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Solid
                    },
                    DepthStencilDesc = new()
                    {
                        DepthEnable = false,
                        DepthWriteEnable = false
                    },

                    NumRenderTargets = 1,
                    RTVFormats = new[] { Diligent.TextureFormat.RGBA8_UNorm },
                    DSVFormat = Diligent.TextureFormat.Unknown
                }
            });

            this.shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);

            var shaderVolumeVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "g_Input");
            if (shaderVolumeVariable != null)
            {
                shaderVolumeVariable.Set(inputTexture, SetShaderResourceFlags.None);
            }

            var lutDesc = new TextureDesc()
            {
                Type = ResourceDimension.Tex1d,        // 1D texture
                Width = 256,                           // 256 entries
                Height = 1,                            // must be 1
                ArraySizeOrDepth = 1,
                MipLevels = 1,
                Format = TextureFormat.RGBA8_UNorm,    // 8-bit RGBA
                BindFlags = BindFlags.ShaderResource,  // used as SRV in shader
                Usage = Usage.Dynamic,
                CPUAccessFlags = CpuAccessFlags.Write
            };

            var lutHandle = GCHandle.Alloc(this.lut == null ? this.defaultLut : this.lut, GCHandleType.Pinned);

            var data = new TextureSubResData()
            {
                Data = lutHandle.AddrOfPinnedObject(),
                Stride = 256 * 4 
            };

            var initData = new TextureData()
            {
                SubResources = new[] { data }
            };

            // Create texture
            this.lutTexture = renderDevice.CreateTexture(lutDesc, initData);
            lutHandle.Free();

            // Create SRV (Shader Resource View)
            this.lutTextureView = lutTexture.GetDefaultView(TextureViewType.ShaderResource);

            var shaderVolumeVariableLut = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "g_LUT");
            if (shaderVolumeVariableLut != null)
            {
                shaderVolumeVariableLut.Set(lutTextureView, SetShaderResourceFlags.None);
            }

        }

        public void Render(ITextureView shaderResourceTextureView, Viewport viewport)
        {
            CreatePipeline();

            deviceContext.SetPipelineState(pipeline);
            deviceContext.SetViewports(new Diligent.Viewport[] { viewport }, 0, 0);

            this.shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "g_Input").Set(shaderResourceTextureView, SetShaderResourceFlags.None);

            deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);

            deviceContext.Draw(new DrawAttribs { NumVertices = 3 });
        }

        public void Dispose()
        {
        }
    }
}

using Diligent;
using PragmaticScene.Scene;
using PragmaticScene.RenderableInterfaces;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DupploPulse.UsImaging.ImagePostProcessing.ImageProcessors
{
    internal class ScanConverter
    {
        private readonly IRenderDevice renderDevice;
        private readonly Diligent.IDeviceContext deviceContext;
        private readonly IEngineFactory engineFactory;
        private readonly IBuffer rawUsData;
        private readonly IBuffer vertexBuffer;
        private readonly IBuffer indexBuffer;
        private readonly IBuffer uniformBuffer;
        private readonly ITextureView inputTexture;

        private IShaderSourceInputStreamFactory shaderSourceFactory;

        private IPipelineState pipeline;
        private IShaderResourceBinding shaderResourceBinding;


        struct ScanConverterDesc
        {
            public float MmPerPixel;
            public float CenterX;
            public float CenterY;
        }

        public ScanConverter(IRenderDevice renderDevice, Diligent.IDeviceContext deviceContext, IEngineFactory engineFactory, ISwapChain swapChain, ITextureView inputTexture)
        {
            this.renderDevice = renderDevice;
            this.deviceContext = deviceContext;
            this.engineFactory = engineFactory;
            this.inputTexture = inputTexture;

            this.rawUsData = renderDevice.CreateBuffer(new()
            {
#if LINUX
                ImmediateContextMask = 1,
#endif
                Mode = BufferMode.Raw,
                Name = "us data buffer",
                Size = new SharpGen.Runtime.NativeULong(151792),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            this.vertexBuffer = renderDevice.CreateBuffer(new()
            {
#if LINUX
                ImmediateContextMask = 1,
#endif
                Name = "Model vertex buffer",
                Usage = Usage.Immutable,
                BindFlags = BindFlags.VertexBuffer,
                Size = new SharpGen.Runtime.NativeULong((ulong)(Unsafe.SizeOf<Vector3>() * 4))

            }, new Vector3[] { new Vector3(-1.0f, -2.0f, 0.0f), new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, -1.0f, 0.0f) });

            this.indexBuffer = renderDevice.CreateBuffer(new()
            {
#if LINUX
                ImmediateContextMask = 1,
#endif
                Name = "Model index buffer",
                Usage = Usage.Immutable,
                BindFlags = BindFlags.IndexBuffer,
                Size = new SharpGen.Runtime.NativeULong((ulong)(Unsafe.SizeOf<uint>() * 6))
            }, new uint[] { 0, 1, 2, 0, 2, 3 });

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
            DateTime currentDateTime = File.GetLastWriteTime("UltrasoundShaders//ScanConverterPS.psh");
            if(currentDateTime == lastShaderDateTime)
            {
                return;
            }
            lastShaderDateTime = currentDateTime;

            var vs = renderDevice.CreateShader(new()
            {
                FilePath = "scanConverterVS.vsh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "ScanConverter VS",
                    ShaderType = ShaderType.Vertex,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);

            if(vs == null)
            {
                return;
            }

            var ps = renderDevice.CreateShader(new()
            {
                FilePath = "scanConverterPS.psh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "scanConverter PS",
                    ShaderType = ShaderType.Pixel,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);

            if(ps == null)
            {
                return;
            }

            this.pipeline = renderDevice.CreateGraphicsPipelineState(new()
            {
                PSODesc = new()
                {
                    Name = "Scan Converter PSO",
                    ResourceLayout = new()
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[] {
                            new ()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "usRawData",
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
                    InputLayout = new()
                    {
                        LayoutElements = new[] {
                            new LayoutElement
                            {
                                InputIndex = 0,
                                NumComponents = 3,
                                ValueType = Diligent.ValueType.Float32,
                                IsNormalized = false,
                            }
                        }
                    },
                    PrimitiveTopology = PrimitiveTopology.TriangleList,

                    RasterizerDesc = new() { CullMode = CullMode.None },
                    DepthStencilDesc = new() { DepthEnable = true },
                    NumRenderTargets = 1,
                    RTVFormats = new[] { Diligent.TextureFormat.RGBA8_UNorm }, // todo: remove hardcoded
                    DSVFormat = Diligent.TextureFormat.D32_Float  // todo: remove hardcoded
                    //RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },
                    //DSVFormat = swapChain.GetDesc().DepthBufferFormat
                }
            });

            pipeline.GetStaticVariableByName(ShaderType.Pixel, "Constants").Set(this.uniformBuffer, SetShaderResourceFlags.None);

            this.shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);

            var shaderVolumeVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "usRawData");
            if (shaderVolumeVariable != null)
            {
                shaderVolumeVariable.Set(inputTexture, SetShaderResourceFlags.None);
            }
        }

        private static Vector3 GetCameraPosition(Matrix4x4 viewMatrix)
        {
            Matrix4x4.Invert(viewMatrix, out var invView);
            return new Vector3(invView.M41, invView.M42, invView.M43);
        }

        public static (float width, float height) GetOrthoSize(Matrix4x4 projectionMatrix)
        {
            float width = 2f / projectionMatrix.M11;
            float height = 2f / projectionMatrix.M22;
            return (width, height);
        }

        public void Render(Diligent.Viewport viewport, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            CreatePipeline();

            var mapUniformBuffer = deviceContext.MapBuffer<ScanConverterDesc>(uniformBuffer, MapType.Write, MapFlags.Discard);

            var cameraCenter = GetCameraPosition(viewMatrix);
            var (orthoWidth, orthoHeight) = GetOrthoSize(projectionMatrix);

            float mmPerPixel = orthoHeight / viewport.Height;
            float centerX = viewport.Width / 2 + cameraCenter.X / mmPerPixel;
            float centerY = (cameraCenter.Y - 0.5f) / mmPerPixel - (viewport.Height / 2);  //TODO: check why 0.5f is needed here

            ScanConverterDesc scanConverterDesc = new()
            {
                MmPerPixel = mmPerPixel,
                CenterX = centerX,
                CenterY = centerY
            };

            mapUniformBuffer[0] = scanConverterDesc;
            deviceContext.UnmapBuffer(uniformBuffer, MapType.Write);

            deviceContext.SetPipelineState(pipeline);
           // var fullViewport = new Diligent.Viewport();
           // fullViewport.Width = 1024;
           // fullViewport.Height = 720;

           //deviceContext.SetViewports(new Diligent.Viewport[] { fullViewport }, 0, 0);

            deviceContext.SetVertexBuffers(0, new[] { vertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
            deviceContext.SetIndexBuffer(indexBuffer, 0, ResourceStateTransitionMode.Transition);
            deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);

            deviceContext.DrawIndexed(new()
            {
                IndexType = Diligent.ValueType.UInt32,
                NumIndices = 6,
                Flags = DrawFlags.VerifyAll
            });
        }
    }
}

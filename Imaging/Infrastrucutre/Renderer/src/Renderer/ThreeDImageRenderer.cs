using System;
using System.Collections;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Diligent;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.Infrastructure.Common;
using PragmaticScene.RendererInterfaces;
using PragmaticScene.Scene;
using PragmaticScene.SceneInterfaces;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Renderer
{
    using IDeviceContext = IDeviceContext;

    internal class ThreeDImageRenderer : IProjectedSceneObjectRenderer
    {
        struct Vertex
        {
            public Vector3 Position;
            public Vector3 Color;

            public Vertex(Vector3 position, Vector3 color)
            {
                Position = position;
                Color = color;
            }
        }

        struct WindowSize
        {
            public float Width;
            public float Height;
            public float StartX;
            public float StartY;
        }

        struct VertexShaderConstants
        {
            public Matrix4x4 ViewMatrix;
            public Matrix4x4 RotationMatrix;
        }

        IBuffer uniformBuffer;
        IBuffer windowSizeBuffer;
        IBuffer vertexBuffer;
        IBuffer indexBuffer;

        IPipelineState pipelineCubeFront;
        IPipelineState pipelineCubeBack;
        IPipelineState pipelineRaycaster;

        IShaderResourceBinding shaderResourceBindingBack;
        IShaderResourceBinding shaderResourceBindingFront;
        IShaderResourceBinding shaderResourceBindingRayCasting;

        ITexture frontCubeTexture;
        ITexture backCubeTexture;
        ITexture volumeTexture;

        ITextureView frontCubeTextureSrv;
        ITextureView frontCubeTextureRtv;
        ITextureView backCubeTextureSrv;
        ITextureView backCubeTextureRtv;
        ITextureView volumeTextureSrv;

        Stopwatch clock = new Stopwatch();

        private readonly Diligent.IDeviceContext deviceContext;
        private readonly IRenderDevice renderDevice;
        private readonly IEngineFactory engineFactory;
        private readonly IRenderableThreeDImageSceneObject threeDImage;

        private DateTime lastShaderDateTime = DateTime.MinValue;
        private Vector2 lastViewportSize = Vector2.Zero;

        public PragmaticScene.RenderableInterfaces.IRenderableSceneObject SceneObject => threeDImage;

        public ThreeDImageRenderer(IRenderDevice renderDevice, Diligent.IDeviceContext deviceContext, IEngineFactory engineFactory, IRenderableThreeDImageSceneObject threeDImage)
        {
            this.renderDevice = renderDevice;
            this.deviceContext = deviceContext;
            this.engineFactory = engineFactory;
            this.threeDImage = threeDImage;

            const float boxWidth = 2.0f;
            const float boxHeight = 2.0f;
            const float boxDepth = 2.0f;

            const float boxTopLeftFrontCornerX = 1.0f;
            const float boxTopLeftFrontCornerY = 1.0f;
            const float boxTopLeftFrontCornerZ = 1.0f;

            var cubeVertices = new Vertex[] {
                new Vertex(new(boxTopLeftFrontCornerX - boxWidth, boxTopLeftFrontCornerY - boxHeight, boxTopLeftFrontCornerZ - boxDepth), new (0,0,0)),
                new Vertex(new(boxTopLeftFrontCornerX - boxWidth, boxTopLeftFrontCornerY - boxHeight, boxTopLeftFrontCornerZ), new(0, 0, 1)),
                new Vertex(new(boxTopLeftFrontCornerX - boxWidth, boxTopLeftFrontCornerY, boxTopLeftFrontCornerZ - boxDepth), new (0,1,0)),
                new Vertex(new(boxTopLeftFrontCornerX - boxWidth, boxTopLeftFrontCornerY, boxTopLeftFrontCornerZ), new (0,1,1)),
                new Vertex(new(boxTopLeftFrontCornerX, boxTopLeftFrontCornerY - boxHeight, boxTopLeftFrontCornerZ - boxDepth), new (1,0,0)),
                new Vertex(new(boxTopLeftFrontCornerX , boxTopLeftFrontCornerY - boxHeight, boxTopLeftFrontCornerZ), new (1,0,1)),
                new Vertex(new(boxTopLeftFrontCornerX, boxTopLeftFrontCornerY, boxTopLeftFrontCornerZ - boxDepth), new (1,1,0)),
                new Vertex(new(boxTopLeftFrontCornerX, boxTopLeftFrontCornerY, boxTopLeftFrontCornerZ) , new (1,1,1) )
            };

            var cubeIndices = new uint[]
            {
                0, 1, 2,
                2, 1, 3,
                0, 4, 1,
                1, 4, 5,
                0, 2, 4,
                4, 2, 6,
                1, 5, 3,
                3, 5, 7,
                2, 3, 6,
                6, 3, 7,
                5, 4, 7,
                7, 4, 6,
            };

            using var shaderSourceFactory = engineFactory.CreateDefaultShaderSourceStreamFactory("UltrasoundShaders");

            // Create buffers
            vertexBuffer = renderDevice.CreateBuffer(new()
            {
                Name = "Cube vertex buffer",
                Usage = Usage.Immutable,
                BindFlags = BindFlags.VertexBuffer,
                Size = (ulong)(Unsafe.SizeOf<Vertex>() * cubeVertices.Length)
            }, cubeVertices);

            indexBuffer = renderDevice.CreateBuffer(new()
            {
                Name = "Cube index buffer",
                Usage = Usage.Immutable,
                BindFlags = BindFlags.IndexBuffer,
                Size = (ulong)(Unsafe.SizeOf<uint>() * cubeIndices.Length)
            }, cubeIndices);

            uniformBuffer = renderDevice.CreateBuffer(new()
            {
                Name = "Uniform buffer",
                Size = (ulong)Unsafe.SizeOf<VertexShaderConstants>(),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            windowSizeBuffer = renderDevice.CreateBuffer(new()
            {
                Name = "Window size buffer",
                Size = (ulong)Unsafe.SizeOf<WindowSize>(),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });


            // Back and front cube pipelines
            var vs = renderDevice.CreateShader(new()
            {
                FilePath = "cube.vsh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "Cube VS",
                    ShaderType = ShaderType.Vertex,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);

            var ps = renderDevice.CreateShader(new()
            {
                FilePath = "cube.psh",
                ShaderSourceStreamFactory = shaderSourceFactory,
                Desc = new()
                {
                    Name = "Cube PS",
                    ShaderType = ShaderType.Pixel,
                    UseCombinedTextureSamplers = true
                },
                SourceLanguage = ShaderSourceLanguage.Hlsl
            }, out _);


            pipelineCubeFront = renderDevice.CreateGraphicsPipelineState(new()
            {
                PSODesc = new() { Name = "pipelineCubeFront" },
                Vs = vs,
                Ps = ps,
                GraphicsPipeline = new()
                {
                    InputLayout = new()
                    {
                        LayoutElements = new[] {
                        new LayoutElement
                        {
                            InputIndex = 0,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        },
                        new LayoutElement
                        {
                            InputIndex = 1,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        }
                    }
                    },
                    PrimitiveTopology = PrimitiveTopology.TriangleList,
                    RasterizerDesc = new() { CullMode = CullMode.Front },
                    DepthStencilDesc = new() { DepthEnable = true },
                    NumRenderTargets = 1,
                }
            });

            pipelineCubeFront.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(uniformBuffer, SetShaderResourceFlags.None);
            shaderResourceBindingFront = pipelineCubeFront.CreateShaderResourceBinding(true);

            pipelineCubeBack = renderDevice.CreateGraphicsPipelineState(new()
            {
                PSODesc = new() { Name = "pipelineCubeBack" },
                Vs = vs,
                Ps = ps,
                GraphicsPipeline = new()
                {
                    InputLayout = new()
                    {
                        LayoutElements = new[] {
                        new LayoutElement
                        {
                            InputIndex = 0,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        },
                        new LayoutElement
                        {
                            InputIndex = 1,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        }
                    }
                    },
                    PrimitiveTopology = PrimitiveTopology.TriangleList,
                    RasterizerDesc = new() { CullMode = CullMode.Back },
                    DepthStencilDesc = new() { DepthEnable = true },
                    NumRenderTargets = 1,
                }
            });

            pipelineCubeBack.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(uniformBuffer, SetShaderResourceFlags.None);
            shaderResourceBindingBack = pipelineCubeBack.CreateShaderResourceBinding(true);

            // Raycasting pipeline
            volumeTexture = TmpLoadVolume(renderDevice, deviceContext, "assets/usd_heart_256x256x256_uint8.raw");
            this.volumeTextureSrv = volumeTexture.GetDefaultView(TextureViewType.ShaderResource);

            clock.Start();
        }

        private void CreateOrUpdateRayCastingPipeline(int width, int height)
        {
            DateTime currentDateTime = File.GetLastWriteTime("UltrasoundShaders//rayCasting.psh");

            if (currentDateTime != lastShaderDateTime)
            {
                CreateOrUpdateTextureViews(width, height);

                lastShaderDateTime = currentDateTime;
                lastViewportSize = new Vector2(width, height);

                using var shaderSourceFactory = engineFactory.CreateDefaultShaderSourceStreamFactory("UltrasoundShaders");

                IPipelineState updatedPipeline = null;

                try
                {
                    var rayCastingVS = renderDevice.CreateShader(new()
                    {
                        FilePath = "rayCasting.vsh",
                        ShaderSourceStreamFactory = shaderSourceFactory,
                        Desc = new()
                        {
                            Name = "Ray tracing VS",
                            ShaderType = ShaderType.Vertex,
                            UseCombinedTextureSamplers = true,

                        },
                        SourceLanguage = ShaderSourceLanguage.Hlsl,

                    }, out _);

                    var rayCastingPS = renderDevice.CreateShader(new()
                    {
                        FilePath = "rayCasting.psh",
                        ShaderSourceStreamFactory = shaderSourceFactory,
                        Desc = new()
                        {
                            Name = "Ray casting PS",
                            ShaderType = ShaderType.Pixel,
                            UseCombinedTextureSamplers = true
                        },
                        SourceLanguage = ShaderSourceLanguage.Hlsl,

                    }, out _);

                    var rayCastingPipelineStateDesc = new PipelineStateDesc();
                    rayCastingPipelineStateDesc.Name = "RayCasting PSO";
                    rayCastingPipelineStateDesc.ResourceLayout = new()
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[] {
                        new ()
                        {
                            ShaderStages = ShaderType.Pixel,
                            Name = "txVolume",
                            Type = ShaderResourceVariableType.Mutable,

                        },
                        new ()
                        {
                            ShaderStages = ShaderType.Pixel,
                            Name = "txPositionFront",
                            Type = ShaderResourceVariableType.Mutable,

                        },
                        new ()
                        {
                            ShaderStages = ShaderType.Pixel,
                            Name = "txPositionBack",
                            Type = ShaderResourceVariableType.Mutable,

                        }
                        },
                        ImmutableSamplers = new ImmutableSamplerDesc[] {
                        new ()
                        {
                            Desc = new()
                            {
                                MinFilter = FilterType.Linear, MagFilter = FilterType.Linear, MipFilter = FilterType.Linear,
                                AddressU = TextureAddressMode.Border, AddressV = TextureAddressMode.Border, AddressW = TextureAddressMode.Border,
                                ComparisonFunc = ComparisonFunction.Never,
                                MinLOD = 0,
                            },
                            SamplerOrTextureName = "txPositionFront",
                            ShaderStages = ShaderType.Pixel
                        } }
                    };

                    updatedPipeline = renderDevice.CreateGraphicsPipelineState(new()
                    {
                        PSODesc = rayCastingPipelineStateDesc,
                        Vs = rayCastingVS,
                        Ps = rayCastingPS,
                        GraphicsPipeline = new()
                        {
                            InputLayout = new()
                            {
                                LayoutElements = new[] {
                        new LayoutElement
                        {
                            InputIndex = 0,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        },
                        new LayoutElement
                        {
                            InputIndex = 1,
                            NumComponents = 3,
                            ValueType = Diligent.ValueType.Float32,
                            IsNormalized = false,
                        }
                    }
                            },
                            PrimitiveTopology = PrimitiveTopology.TriangleList,
                            RasterizerDesc = new() { CullMode = CullMode.Front },
                            DepthStencilDesc = new() { DepthEnable = true },
                            NumRenderTargets = 1
                        }
                    });

                    updatedPipeline.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(uniformBuffer, SetShaderResourceFlags.None);
                    updatedPipeline.GetStaticVariableByName(ShaderType.Pixel, "Constants").Set(windowSizeBuffer, SetShaderResourceFlags.None);
                }
                catch
                {
                    if (this.pipelineRaycaster == null)
                    {
                        throw;
                    }
                    else
                    {
                        return;
                    }
                }

                this.pipelineRaycaster?.Dispose();
                this.pipelineRaycaster = updatedPipeline;

                shaderResourceBindingRayCasting = pipelineRaycaster.CreateShaderResourceBinding(true);

                var shaderVolumeVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txVolume");
                shaderVolumeVariable.Set(volumeTextureSrv, SetShaderResourceFlags.None);

                var shaderFrontVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txPositionFront");
                shaderFrontVariable.Set(frontCubeTextureSrv, SetShaderResourceFlags.None);

                var shaderBackVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txPositionBack");
                shaderBackVariable.Set(backCubeTextureSrv, SetShaderResourceFlags.None);
            }

            if (lastViewportSize.X != width && lastViewportSize.Y != height)
            {
                lastViewportSize = new Vector2(width, height);

                CreateOrUpdateTextureViews(width, height);


                shaderResourceBindingRayCasting = this.pipelineRaycaster.CreateShaderResourceBinding(true);

                var shaderVolumeVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txVolume");
                shaderVolumeVariable.Set(volumeTextureSrv, SetShaderResourceFlags.None);

                var shaderFrontVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txPositionFront");
                shaderFrontVariable.Set(frontCubeTextureSrv, SetShaderResourceFlags.None);

                var shaderBackVariable = shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txPositionBack");
                shaderBackVariable.Set(backCubeTextureSrv, SetShaderResourceFlags.None);
            }
        }

        private bool pdone = false;

        public void Render(ITextureView rtv, ITextureView dsv, in ViewProjectionData viewProjectionData)
        {
            CreateOrUpdateRayCastingPipeline((int)viewProjectionData.Viewport.Width, (int)viewProjectionData.Viewport.Height);

            // Uniform buffer for mvp
            var worldMatrix = Matrix4x4.Identity;
            var wvpMatrix = Matrix4x4.Transpose(worldMatrix * viewProjectionData.ViewMatrix * viewProjectionData.ProjectionMatrix);
            var mapUniformBuffer = deviceContext.MapBuffer<VertexShaderConstants>(uniformBuffer, MapType.Write, MapFlags.Discard);

            Matrix4x4 inverted;
            Matrix4x4.Invert(viewProjectionData.ViewMatrix, out inverted);

            VertexShaderConstants vpData = new VertexShaderConstants
            {
                ViewMatrix = wvpMatrix,
                RotationMatrix = inverted,
            };

            mapUniformBuffer[0] = vpData;
            deviceContext.UnmapBuffer(uniformBuffer, MapType.Write);

            // Copy volume data
            UpdateVolumeTexture();

            //Window size buffer
            var mapWindowSizeBuffer = deviceContext.MapBuffer<WindowSize>(windowSizeBuffer, MapType.Write, MapFlags.Discard);
            mapWindowSizeBuffer[0] = new WindowSize() { Width = 1 / viewProjectionData.Viewport.Width, Height = 1 / viewProjectionData.Viewport.Height, StartX = viewProjectionData.Viewport.TopLeftX, StartY = viewProjectionData.Viewport.TopLeftY };

            deviceContext.UnmapBuffer(windowSizeBuffer, MapType.Write);

            Diligent.Viewport viewportAdjusted = new Viewport();
            viewportAdjusted.TopLeftX = 0;
            viewportAdjusted.TopLeftX = 0;
            viewportAdjusted.Width = viewProjectionData.Viewport.Width;
            viewportAdjusted.Height = viewProjectionData.Viewport.Height;
            viewportAdjusted.MinDepth = viewProjectionData.Viewport.MinDepth;
            viewportAdjusted.MaxDepth = viewProjectionData.Viewport.MaxDepth;

            pdone = false;
            if (pdone == false)
            {
                pdone = false;

                // Back cube pipeline 
                deviceContext.SetPipelineState(pipelineCubeBack);
                deviceContext.SetVertexBuffers(0, new[] { vertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
                deviceContext.SetIndexBuffer(indexBuffer, 0, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(shaderResourceBindingBack, ResourceStateTransitionMode.Transition);
                deviceContext.ClearRenderTarget(backCubeTextureRtv, new(0.0f, 0.0f, 0.0f, 1.0f), ResourceStateTransitionMode.Transition);
                deviceContext.SetRenderTargets(new[] { backCubeTextureRtv }, null, ResourceStateTransitionMode.Transition);

                 deviceContext.SetViewports(new Diligent.Viewport[] { viewportAdjusted }, 0, 0);

                deviceContext.DrawIndexed(new()
                {
                    IndexType = Diligent.ValueType.UInt32,
                    NumIndices = 36,
                    Flags = DrawFlags.VerifyAll
                });

                // Front cube pipeline
                deviceContext.SetPipelineState(pipelineCubeFront);
                deviceContext.SetVertexBuffers(0, new[] { vertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
                deviceContext.SetIndexBuffer(indexBuffer, 0, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(shaderResourceBindingFront, ResourceStateTransitionMode.Transition);
                deviceContext.ClearRenderTarget(frontCubeTextureRtv, new(0.0f, 0.0f, 0.0f, 1.0f), ResourceStateTransitionMode.Transition);
                deviceContext.SetRenderTargets(new[] { frontCubeTextureRtv }, null, ResourceStateTransitionMode.Transition);

                // deviceContext.SetRenderTargets(new[] { rtv }, null, ResourceStateTransitionMode.Transition);

                deviceContext.SetViewports(new Diligent.Viewport[] { viewportAdjusted }, 0, 0);

                deviceContext.DrawIndexed(new()
                {
                    IndexType = Diligent.ValueType.UInt32,
                    NumIndices = 36,
                    Flags = DrawFlags.VerifyAll
                });

            }

            // Ray casting pipeline
            deviceContext.SetPipelineState(pipelineRaycaster);
            deviceContext.SetVertexBuffers(0, new[] { vertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
            deviceContext.SetIndexBuffer(indexBuffer, 0, ResourceStateTransitionMode.Transition);
           //deviceContext.ClearRenderTarget(rtv, new(0.0f, 0.0f, 0.0f, 1.0f), ResourceStateTransitionMode.Transition);
            deviceContext.SetRenderTargets(new[] { rtv }, null, ResourceStateTransitionMode.Transition);

            deviceContext.SetViewports(new Diligent.Viewport[] { viewProjectionData.Viewport }, 0, 0);

            deviceContext.CommitShaderResources(shaderResourceBindingRayCasting, ResourceStateTransitionMode.Transition);

            deviceContext.DrawIndexed(new()
            {
                IndexType = Diligent.ValueType.UInt32,
                NumIndices = 36,
                Flags = DrawFlags.VerifyAll
            });
        }

        private void CreateOrUpdateTextureViews(int width, int height)
        {
            if (frontCubeTexture != null)
            {
                frontCubeTexture.Dispose();
                backCubeTexture.Dispose();
            }

            var cubeTextureDesritpion = new TextureDesc()
            {
                Type = ResourceDimension.Tex2d,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
                Format = TextureFormat.RGBA32_Float,
                Height = (uint)height,
                Width = (uint)width,

            };

            frontCubeTexture = this.renderDevice.CreateTexture(cubeTextureDesritpion);
            frontCubeTextureSrv = frontCubeTexture.GetDefaultView(TextureViewType.ShaderResource);
            frontCubeTextureRtv = frontCubeTexture.GetDefaultView(TextureViewType.RenderTarget);

            backCubeTexture = this.renderDevice.CreateTexture(cubeTextureDesritpion);
            backCubeTextureSrv = backCubeTexture.GetDefaultView(TextureViewType.ShaderResource);
            backCubeTextureRtv = backCubeTexture.GetDefaultView(TextureViewType.RenderTarget);


        }

        public void Dispose()
        {
            uniformBuffer.Dispose();
            windowSizeBuffer.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();

            frontCubeTexture.Dispose();
            backCubeTexture.Dispose();
            volumeTexture.Dispose();

            shaderResourceBindingBack.Dispose();
            shaderResourceBindingFront.Dispose();
            shaderResourceBindingRayCasting.Dispose();

            pipelineCubeFront.Dispose();
            pipelineCubeBack.Dispose();
            pipelineRaycaster.Dispose();

        }

        public static Matrix4x4 TmpCreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isOpenGL)
        {
            if (fieldOfView <= 0.0f || fieldOfView >= MathF.PI)
                throw new ArgumentOutOfRangeException(nameof(fieldOfView));

            if (nearPlaneDistance <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

            if (farPlaneDistance <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

            if (nearPlaneDistance >= farPlaneDistance)
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

            float yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4x4 result = new()
            {
                M11 = xScale,
                M22 = yScale
            };

            if (isOpenGL)
            {
                result.M33 = (farPlaneDistance + nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
                result.M43 = -2 * nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M34 = 1.0f;
            }
            else
            {
                result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M43 = -nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M34 = 1.0f;
            }

            return result;
        }

        private void UpdateVolumeTexture()
        {
            var managedMemoryFrameReference = this.threeDImage.ImageData as ManagedMemoryFrameReference;
            var geometryData = this.threeDImage.GeometryData as VolumeCartezianGeometry?;

            var height = (uint)(geometryData?.Height ?? 256);
            var width = (uint)(geometryData?.Width ?? 256);
            var depth = (uint)(geometryData?.Depth ?? 256);

            if (volumeTexture == null ||
               volumeTexture.GetDesc().Height != height ||
               volumeTexture.GetDesc().Width != width ||
               volumeTexture.GetDesc().ArraySizeOrDepth != depth)
            {
                volumeTexture?.Dispose();

                var descritpion = new TextureDesc
                {
                    Type = ResourceDimension.Tex3d,
                    Name = "Volume",
                    Height = depth,
                    Width = width,
                    ArraySizeOrDepth = height,
                    Format = TextureFormat.R8_UNorm,
                    Usage = Usage.Dynamic,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.Write
                };

                unsafe
                {
                    fixed (byte* ptr = managedMemoryFrameReference.FrameBuffer)
                    {

                        var rawData = new TextureData
                        {
                            Context = deviceContext,
                            SubResources = new TextureSubResData[]
                             {
                                new()
                                {
                                    Data = (IntPtr)ptr,
                                    Stride = descritpion.Width,
                                    DepthStride = descritpion.Height * descritpion.Width
                                }
                            }

                        };
                        this.volumeTexture = this.renderDevice.CreateTexture(descritpion, rawData);

                    }
                }

                
                this.volumeTextureSrv = volumeTexture.GetDefaultView(TextureViewType.ShaderResource);

                var shaderVolumeVariable = this.shaderResourceBindingRayCasting.GetVariableByName(ShaderType.Pixel, "txVolume");
                shaderVolumeVariable.Set(volumeTextureSrv, SetShaderResourceFlags.None);
            }

            var imageReference = threeDImage.ImageData as ManagedMemoryFrameReference;

         //   var mappedTexture = deviceContext.MapTextureSubresource(volumeTextureSrv.GetTexture(), 0, 0, MapType.Write, MapFlags.Discard, null);

            if (imageReference == null)
            {
                //TODO: update buffer to have zeroes 256x256x256
                // Marshal.Copy(ClearImage, 0, mappedTexture.Data, (int)(TextureWidth * TextureHeight));
            }
            else
            {
              //  Marshal.Copy(imageReference.FrameBuffer, 0, mappedTexture.Data, (int)height * (int)width * (int)depth);
            }

      //      deviceContext.UnmapTextureSubresource(volumeTextureSrv.GetTexture(), 0, 0);

        }

        public static ITexture TmpLoadVolume(IRenderDevice device, IDeviceContext deviceContext, string fileName)
        {
            var descritpion = new TextureDesc
            {
                Type = ResourceDimension.Tex3d,
                Name = "Volume",
                Height = 256,
                Width = 256,
                ArraySizeOrDepth = 256,
                Format = TextureFormat.R8_UNorm,
                Usage = Usage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.Write
            };

            using var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
            using var accessor = mmf.CreateViewAccessor();

            long fileSize = new FileInfo(fileName).Length;
            byte[] byteArray = new byte[fileSize];
            accessor.ReadArray(0, byteArray, 0, (int)fileSize);

            // Initialize the volume data with some pattern if needed for debugging
            //for (int i = 0; i < byteArray.Length; i++)
            //{
            //    if (((i / 5) % 2) == 1)
            //    {
            //        byteArray[i] = 100;
            //    }

            //}

            unsafe
            {
                fixed (byte* ptr = byteArray)
                {
                    var rawData = new TextureData
                    {
                        Context = deviceContext,
                        SubResources = new TextureSubResData[]
                            {
                                new()
                                {
                                    Data = (IntPtr)ptr,
                                    Stride = 256,
                                    DepthStride = 256*256
                                }
                            }
                    };
                    var texture = device.CreateTexture(descritpion, rawData);
                    return texture;
                }
            }
        }
    }

}
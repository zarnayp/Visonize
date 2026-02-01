using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.SceneObjects;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.Renderer.RendererInterfaces;
using PragmaticScene.RendererInterfaces;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Renderer
{
    public class UsRenderersProviderCreator : IRenderersProviderCreator
    {
        public IRenderersProvider CreateRenderersProvider(RendererAssets rendererAssets)
        {
            return new UsRenderersProvider(rendererAssets);
        }
    }

    internal class UsRenderersProvider : IRenderersProvider
    {

        private readonly List<IProjectedSceneObjectRenderer> renderers = new List<IProjectedSceneObjectRenderer>();
        private RendererAssets rendererAssets;

        public UsRenderersProvider(RendererAssets rendererAssets)
        {
            this.rendererAssets = rendererAssets;
        }

        public bool AssignToRenderer(IRenderableSceneObject renderableSceneObject)
        {
            if (renderableSceneObject is IRenderableTwoDImageSceneObject renderableTwoDImageSceneObject)
            {
                var renderer = new TwoDImageRenderer(this.rendererAssets.RenderDevice, this.rendererAssets.DeviceContext, this.rendererAssets.EngineFactory, this.rendererAssets.SwapChain, renderableTwoDImageSceneObject);
                renderers.Add(renderer);

                return true;
            }

            if (renderableSceneObject is IRenderableThreeDImageSceneObject renderableThreeDImageSceneObject)
            {
                var renderer = new ThreeDImageRenderer(this.rendererAssets.RenderDevice, this.rendererAssets.DeviceContext, this.rendererAssets.EngineFactory, renderableThreeDImageSceneObject);
                renderers.Add(renderer);

                return true;

            }

            return false;
        }

        public IReadOnlyList<IProjectedSceneObjectRenderer> GetRenderers()
        {
            return [.. this.renderers];

        }

        public void RemoveFromRenderer(IRenderableSceneObject renderableSceneObject)
        {
            throw new NotImplementedException();
        }
    }
}

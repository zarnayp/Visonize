using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.RendererInterfaces;
using PragmaticScene.RenderableInterfaces;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Scene
{
    public class SceneObjectRenderersCreator : ISceneObjectRenderersCreator
    {
        public ISceneObjectRenderer? CreateRenderer(RendererAssets assets, IRenderableSceneObject sceneObject)
        {
            if (sceneObject is ITwoDImageSceneObject twoDImage)
            {
                return new TwoDImageRenderer(assets.RenderDevice, assets.DeviceContext, assets.EngineFactory, assets.SwapChain, twoDImage);
            }
            else
            {
                return null;
            }
        }
    }
}

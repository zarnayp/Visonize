using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standalone
{
    using Visonize.UsImaging.Domain.Service.Infrastructure;
    using Visonize.UsImaging.Infrastructure.Common;
  //  using global::Visonize.UsImaging.Domain.Service.Infrastructure;

    public class RendererAdapter : IRenderer
    {
        private readonly PragmaticScene.SceneInterfaces.ISceneRenderer renderer;

        public RendererAdapter(PragmaticScene.SceneInterfaces.ISceneRenderer renderer)
        {
            this.renderer = renderer;
        }

        public PragmaticScene.SceneInterfaces.ISceneRenderer PragmaticSceneRenderer => this.renderer;

        public void Render()
        {
            this.renderer.Render();
        }

        public void RenderWithReadback(IRgbImageReference imageReference)
        {
            var buffer = ((ManagedHeapRGBImageReference)imageReference).Rgb;

            this.renderer.RenderWithReadback(buffer);
        }

        //public IRgbImageReference GetLatestData()
        //{
        //  //  return new ManagedHeapRGBImageReference(this.renderer.GetLatestData());
        //}
    }
}

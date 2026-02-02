using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;

namespace Visonize.UsImaging.Domain.Entities.SceneBehaviour
{
    internal class Camera2DViewportBehaviour : IViewportChangeBehaviour
    {
        private OrthographicCamera camera;

        public Camera2DViewportBehaviour(OrthographicCamera camera)
        {
            this.camera = camera;
        }

        public void OnViewportChange(IBehaviourInput input)
        {
            float viewportAspect = (float)input.ViewInput.ViewportWidth / (float)input.ViewInput.ViewportHeight;
            float camHeight = camera.Height;
            float camWidth = camHeight * viewportAspect;
            this.camera.SetWidthHeight(camWidth, camHeight);
        }
    }
}

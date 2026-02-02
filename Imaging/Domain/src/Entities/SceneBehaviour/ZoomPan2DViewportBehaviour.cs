using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;

namespace Visonize.UsImaging.Domain.Entities.SceneBehaviour
{
    internal class ZoomPan2DViewportBehaviour : IMouseBehaviour
    {
        private OrthographicCamera camera;
        private float? lastMouseX;


        public ZoomPan2DViewportBehaviour(OrthographicCamera camera)
        {
            this.camera = camera;
        }

        public void OnMouseDown(in BehaviourInput input)
        {
            //throw new NotImplementedException();
        }


        public void OnMouseMove(in BehaviourInput input)
        {
            if(this.lastMouseX == null)
            {
                this.lastMouseX = input.MouseInput.X;
                return;
            }

            if (input.MouseInput.Button.HasFlag(MouseButtons.Right))
            {
                   var delta = (float)(this.lastMouseX - input.MouseInput.X);
                   float aspectRatio = this.camera.Width / this.camera.Height;

                   float camHeight = this.camera.Height + (delta * 0.5f);
                float camWidth = camHeight * aspectRatio;
                this.camera.SetWidthHeight(camWidth, camHeight);
            }

            this.lastMouseX = input.MouseInput.X;
        }

        public void OnMouseUp(in BehaviourInput input)
        {
            // no-op
        }
    }

    
}

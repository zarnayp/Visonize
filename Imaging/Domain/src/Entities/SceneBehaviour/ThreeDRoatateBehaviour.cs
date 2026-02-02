using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;

namespace Visonize.UsImaging.Domain.Entities.SceneBehaviour
{
    internal class ThreeDRoatateBehaviour : IMouseBehaviour
    {
        private OrthographicCamera camera;
        private float? lastMouseX;
        private float? lastMouseY;


        public ThreeDRoatateBehaviour(OrthographicCamera camera)
        {
            this.camera = camera;
        }

        public void OnMouseDown(in BehaviourInput input)
        {
            //throw new NotImplementedException();
        }


        public void OnMouseMove(in BehaviourInput input)
        {
            if (this.lastMouseX == null)
            {
                this.lastMouseX = input.MouseInput.X;
                this.lastMouseY = input.MouseInput.Y;

                return;
            }

            if (input.MouseInput.Button.HasFlag(MouseButtons.Right))
            {
                var delta = (float)(this.lastMouseX - input.MouseInput.X);
                float aspectRatio = this.camera.Width / this.camera.Height;

                float camHeight = this.camera.Height + (delta * 0.02f);
                float camWidth = camHeight * aspectRatio;
                this.camera.SetWidthHeight(camWidth, camHeight);
            }

            if (input.MouseInput.Button.HasFlag(MouseButtons.Left))
            {
                var delta = (float)(this.lastMouseX - input.MouseInput.X);
                var deltaY = (float)(this.lastMouseY - input.MouseInput.Y);


                Vector3 target = new Vector3(0, 0, 0);
                Vector3 offset = camera.Position - target;

                Matrix4x4 rotation = Matrix4x4.CreateFromAxisAngle(camera.CameraRotationVectors.UpVector, delta/200);
                Matrix4x4 rotationY = Matrix4x4.CreateFromAxisAngle(camera.CameraRotationVectors.RightVector, deltaY / 200);

                Vector3 rotatedOffset = Vector3.Transform(offset, rotation * rotationY);

                var roatatedPosition = target + rotatedOffset;
                this.camera.SetView(roatatedPosition,Vector3.Normalize(roatatedPosition - target), camera.CameraRotationVectors.UpVector);

            }

            this.lastMouseX = input.MouseInput.X;
            this.lastMouseY = input.MouseInput.Y;

        }

        public void OnMouseUp(in BehaviourInput input)
        {
            // no-op
        }
    }

}

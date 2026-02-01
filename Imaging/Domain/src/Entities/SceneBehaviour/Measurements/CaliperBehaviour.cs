using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.SceneControl;
using PragmaticScene.SceneInterfaces;

namespace DupploPulse.UsImaging.Domain.Entities.SceneBehaviour.Measurements
{
    internal class CaliperBehaviour : IMouseBehaviour
    {
        private ICreator creator;
        private IScene scene;

        private bool isDefining = false;
        private ILine2D? definingLine = null;
        private ILine2D? definingCrossHorizontalLine = null;
        private ILine2D? definingCrossVerticalLine = null;

        private const float LineThickness = 2.0f;

        public CaliperBehaviour(ICreator creator, IScene scene)
        {
            this.creator = creator;
            this.scene = scene;

        }

        public void OnMouseDown(in BehaviourInput input)
        {
            if (input.MouseInput.Button == MouseButtons.Left)
            {
                this.definingLine = creator.Create2DSceneObject<ILine2D>(Space2D.WorldCoordinatesPixelWidth);
                var pos = input.ViewInput.PositionToWorld2D(input.MouseInput.Location);

                definingLine.StartPoint = new(pos.X, pos.Y);
                definingLine.EndPoint = new(pos.X, pos.Y);
                definingLine.Thickness = LineThickness;

                var crossLineVertical = creator.Create2DSceneObject<ILine2D>(Space2D.WorldCoordinatesPixelWidth);
                crossLineVertical.StartPoint = new(pos.X, pos.Y-1);
                crossLineVertical.EndPoint = new(pos.X, pos.Y+1);
                crossLineVertical.Thickness = LineThickness;

                var crossLineHorizontal = creator.Create2DSceneObject<ILine2D>(Space2D.WorldCoordinatesPixelWidth);
                crossLineHorizontal.StartPoint = new(pos.X - 1, pos.Y);
                crossLineHorizontal.EndPoint = new(pos.X + 1, pos.Y);
                crossLineHorizontal.Thickness = LineThickness;

                this.definingCrossVerticalLine = creator.Create2DSceneObject<ILine2D>(Space2D.WorldCoordinatesPixelWidth);
                this.definingCrossVerticalLine.StartPoint = new(pos.X, pos.Y - 1);
                this.definingCrossVerticalLine.EndPoint = new(pos.X, pos.Y + 1);
                this.definingCrossVerticalLine.Thickness = LineThickness;

                this.definingCrossHorizontalLine = creator.Create2DSceneObject<ILine2D>(Space2D.WorldCoordinatesPixelWidth);
                this.definingCrossHorizontalLine.StartPoint = new(pos.X - 1, pos.Y);
                this.definingCrossHorizontalLine.EndPoint = new(pos.X + 1, pos.Y);
                this.definingCrossHorizontalLine.Thickness = LineThickness;

                this.scene.AddSceneObject(this.definingLine);
                this.scene.AddSceneObject(crossLineVertical);
                this.scene.AddSceneObject(crossLineHorizontal);
                this.scene.AddSceneObject(this.definingCrossVerticalLine);
                this.scene.AddSceneObject(this.definingCrossHorizontalLine);
            }
        }

        public void OnMouseMove(in BehaviourInput input)
        {
            if(this.definingLine != null)
            {
                var pos = input.ViewInput.PositionToWorld2D(input.MouseInput.Location);
                definingLine.EndPoint = new(pos.X, pos.Y);
                this.definingCrossVerticalLine!.StartPoint = new(pos.X, pos.Y - 1);
                this.definingCrossVerticalLine!.EndPoint = new(pos.X, pos.Y + 1);
                this.definingCrossHorizontalLine!.StartPoint = new(pos.X - 1, pos.Y);
                this.definingCrossHorizontalLine!.EndPoint = new(pos.X + 1, pos.Y);
            }


        }

        public void OnMouseUp(in BehaviourInput input)
        {
            this.definingLine = null;
            this.definingCrossHorizontalLine = null;
            this.definingCrossVerticalLine = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.Player;
using Visonize.UsImaging.Domain.SceneObjects;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.SceneInterfaces;
using Visonize.UsImaging.Domain.Entities.SceneBehaviour;
using Visonize.UsImaging.Domain.Entities.SceneBehaviour.Measurements;
using Visonize.UsImaging.Domain.Infrastracture.SceneObjects;

namespace Visonize.UsImaging.Domain.Entities.Scene
{
    internal class AcousticTwoDViewport : IAcousticTwoDViewport
    {
        private ICinePlayer? cinePlayer = null;
        private ISystemTime systemTime;
        private readonly IViewport viewport;

        //public OrthographicCamera Camera { get; }
        public ITwoDImageSceneObject TwoDImage { get; }

        public IViewport Viewport => this.viewport;

        public ICinePlayer? CinePlayer => this.cinePlayer;

        public ITwoDImagePostProcessing PostProcessing { get; }

        public AcousticTwoDViewport(ISystemTime systemTime, IWindowCallback windowCallback, IMouseCallback mouseCallback, IRendererCallback rendererCallback, ICreator creator, float topLeftX, float topLeftY, float width, float height)
        {
            this.systemTime = systemTime;

            var camera = new OrthographicCamera(150, 180, 10, -10, new Vector3(0, 80, -10), new Vector3(0, 0, 1), new Vector3(0, -1, 0));

            var scene = creator.CreateScene();
            TwoDImage = creator.CreateSceneObject<ITwoDImageSceneObject>();
            scene.AddSceneObject(TwoDImage);

            PostProcessing = new TwoDPostProcessing(TwoDImage);

            this.viewport = creator.CreateViewport(scene, camera);

            var sceneController = new SceneController(camera, viewport, windowCallback, mouseCallback, rendererCallback);
            sceneController.AddSceneBehaviour(new Camera2DViewportBehaviour(camera), true);
            sceneController.AddSceneBehaviour(new ZoomPan2DViewportBehaviour(camera), true);
            sceneController.AddSceneBehaviour(new CaliperBehaviour(creator, scene));

            this.viewport.Set(topLeftX, topLeftY, width, height);
        }

        public void SwitchToCine(ICine cine)
        {
            this.cinePlayer = new CinePlayer(cine, systemTime, this.TwoDImage);
        }

        public void SwitchToLive()
        {
            this.cinePlayer = null;
        }

        public void UpdateImageData(IRawFrameReference imageData, IGeometryData geometry)
        {
            this.TwoDImage.UpdateImageData(imageData, geometry);

        }

        public void SetViewport(float topLeftX, float topLeftY, float width, float height)
        {
            this.viewport.Set(topLeftX, topLeftY, width, height);
        }

        public void Zoom(float zoomFactor)
        {
            //float camHeight = this.Camera.Height + (zoomFactor * 1);
            //UpdateCamera(camHeight);

        }

    }
}

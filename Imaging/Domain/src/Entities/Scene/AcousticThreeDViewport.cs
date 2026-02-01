using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Entities.SceneBehaviour;
using DupploPulse.UsImaging.Domain.Entities.SceneBehaviour.Measurements;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.SceneObjects;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;
using PragmaticScene.SceneInterfaces;

namespace DupploPulse.UsImaging.Domain.Entities.Scene
{
    internal class AcousticThreeDViewport
    {
        private ISystemTime systemTime;
        private readonly IViewport viewport;

        public IThreeDImageSceneObject ThreeDImage { get; }

        public IViewport Viewport => this.viewport;


        public AcousticThreeDViewport(ISystemTime systemTime, IWindowCallback windowCallback, IMouseCallback mouseCallback, IRendererCallback rendererCallback, ICreator creator, float topLeftX, float topLeftY, float width, float height)
        {
            this.systemTime = systemTime;

            var camera = new OrthographicCamera(4, 3, 100, -100, new Vector3(0, 0, -10), new Vector3(0, 0, -1), new Vector3(0, -1, 0));

            var scene = creator.CreateScene();
            ThreeDImage = creator.CreateSceneObject<IThreeDImageSceneObject>();
            scene.AddSceneObject(ThreeDImage);

            this.viewport = creator.CreateViewport(scene, camera);

            var sceneController = new SceneController(camera, viewport, windowCallback, mouseCallback, rendererCallback);
            sceneController.AddSceneBehaviour(new ThreeDRoatateBehaviour(camera), true);


            this.viewport.Set(topLeftX, topLeftY, width, height);
        }

        public void SetViewport(float topLeftX, float topLeftY, float width, float height)
        {
            this.viewport.Set(topLeftX, topLeftY, width, height);
        }

        public void SetVisibility(bool isVisible)
        {
            this.viewport.SetVisible(isVisible);
        }

        public void SetArchivedDataSource(IArchivedDataSource archivedDataSource)
        {
            var frame = archivedDataSource.GetCurrentFrame();

            this.ThreeDImage.UpdateImageData(frame.RawFrameReference, frame.RawFrameMetaData.GeometryData);
        }
    }
}

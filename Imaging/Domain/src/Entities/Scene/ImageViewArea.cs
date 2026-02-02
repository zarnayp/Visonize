using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PragmaticScene.Scene;
using PragmaticScene.SceneControl;
using PragmaticScene.RenderableInterfaces;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Player;
using Visonize.UsImaging.Domain.SceneObjects;
using Visonize.UsImaging.Domain.Interfaces;
using PragmaticScene.SceneInterfaces;

namespace Visonize.UsImaging.Domain.Entities.Scene
{

    public class ImageViewArea : IImageViewArea
    {
        private ISceneRenderer renderer;
        private ISystemTime systemTime;
        private ICreator creator;
        private AcousticTwoDViewport? twoDViewport0;
        private AcousticTwoDViewport? twoDViewport1;
        private SceneController sceneController;
        private Window renderedWindow;
        private IMouseCallback mouseCallback;
        private bool down = false;
        private float lastX = 0;
        private IList<AcousticTwoDViewport> acousticTwoDViewport = new List<AcousticTwoDViewport>();
        private IList<AcousticThreeDViewport> acousticThreeDViewports = new List<AcousticThreeDViewport>();

        private IList<AcousticThreeDViewport> acousticThreeDViewportsPool = new List<AcousticThreeDViewport>();

        private Dictionary<int, object> indexedViewports = new Dictionary<int, object>(); // TODO: do not use object

        public IList<IAcousticTwoDViewport> TwoDAcousticViewports => (IList<IAcousticTwoDViewport>)acousticTwoDViewport;

        // todo: remove IRenderer. Let renderer service add and remove viewports
        public ImageViewArea(ISystemTime systemTime, ICreator creator, IMouseCallback mouseCallbck, ISceneRenderer renderer, Window renderedWindow)
        {
            this.systemTime = systemTime;
            this.creator = creator;
            this.renderer = renderer;
            this.renderedWindow = renderedWindow;
            this.mouseCallback = mouseCallbck;

            twoDViewport0 = new AcousticTwoDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, 0, 0, 1, 1);

            var threeDViewport = new AcousticThreeDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, 0, 0, 1, 1);
            this.acousticThreeDViewports.Add(threeDViewport);
            renderer.AddViewport(threeDViewport.Viewport);
            indexedViewports.Add(0, threeDViewport);

        }



        public void MouseMove(float x, float y)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine($"{currentTime} mouse move arrived. {x},{y}");

            if (down)
            {
                if (twoDViewport0.CinePlayer != null)
                {
                    var delta = (int)(lastX - x) / 1;
                    twoDViewport0.CinePlayer?.ScrollFrame(delta);
                }
                else
                {
                    var delta = (float)(lastX - x);
                   //Zoom(delta);
                }
            }
            lastX = x;
        }

        public void MouseDown(float x, float y)
        {
            //twoDViewport0.Set(0.01f, 0, 0.48f, 1);

            //renderer.RemoveViewport(twoDViewport0);

            //twoDViewport1 = new AcousticTwoDViewport(systemTime, renderedWindow, creator, 0.51f, 0, 0.48f, 1);
            //var sceneController = new SceneController(twoDViewport1.Camera, twoDViewport1.Viewport, renderedWindow, renderer);
            //sceneController.AddSceneBehaviour(new TwoDImageBehaviour(twoDViewport1.TwoDImage));
            //renderer.AddViewport(twoDViewport1.Viewport);

            //twoDViewport1.TwoDImage.UpdateImageData(twoDViewport0.TwoDImage.ImageData, twoDViewport0.TwoDImage.GeometryData);

            //down = true;
        }

        public void MouseUp(float x, float y)
        {
            down = false;
        }


        public void UpdateImage(IRawFrameReference rawFremeReference, IGeometryData geometryData)
        {


            twoDViewport0?.UpdateImageData(rawFremeReference, geometryData);
        }

        public void Update()
        {

        }

        public void SwitchToCine(ICine cine)
        {
            twoDViewport0?.SwitchToCine(cine);
        }

        public void SwitchToLive()
        {
            twoDViewport0?.SwitchToLive();
        }

        public void SwitchToReview(ICine cine /*, bookmarks*/)
        {

        }

        private AcousticTwoDViewport Create2DViewport(float topLeftX, float topLeftY, float width, float height)
        {
            var two2DViewport = new AcousticTwoDViewport(this.systemTime, this.renderedWindow, this.mouseCallback , this.renderer, creator, topLeftX, topLeftY, width, height);
            //var sceneController = new SceneController(two2DViewport.Camera, two2DViewport.Viewport, renderedWindow, this.renderer);
            //sceneController.AddSceneBehaviour(new TwoDImageBehaviour(two2DViewport.TwoDImage));

            renderer.AddViewport(twoDViewport0.Viewport);
            acousticTwoDViewport.Add(twoDViewport0);

            return two2DViewport;
        }

        /*
        public void SetViewports(ImageAreaViewport[] viewports)
        {
            foreach (var viewport in this.acousticThreeDViewports)
            {
                renderer.RemoveViewport(viewport.Viewport);
            }
            foreach (var viewport in this.acousticTwoDViewport)
            {
                renderer.RemoveViewport(viewport.Viewport);
            }

            this.acousticThreeDViewports.Clear();

            foreach (var viewport in viewports)
            {
                if (viewport.ViewportType == ViewportType.Acquisition)
                {
                    twoDViewport0 = new AcousticTwoDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                    renderer.AddViewport(twoDViewport0.Viewport);

                    acousticTwoDViewport.Add(twoDViewport0);
                }
                else
                {
                    // twoDViewport0 = new AcousticTwoDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, 0, 0, 1, 1);

                    var threeDViewport = new AcousticThreeDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                    this.acousticThreeDViewports.Add(threeDViewport);
                    renderer.AddViewport(threeDViewport.Viewport);
                }
            }
        }*/

        public void AddViewports(ImageAreaViewportWithContextDTO[] viewports)
        {
            foreach (var viewport in viewports)
            {
                if (viewport.ViewportType == ViewportType.Acquisition)
                {
                    var twoD = new AcousticTwoDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                    renderer.AddViewport(twoD.Viewport);
                    acousticTwoDViewport.Add(twoD);
                }
                else
                {
                    var threeDViewport = new AcousticThreeDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                    this.acousticThreeDViewports.Add(threeDViewport);
                    renderer.AddViewport(threeDViewport.Viewport);

                    if (viewport.Context is IArchivedDataSource archived)
                    {
                        threeDViewport.SetArchivedDataSource(archived);
                    }
                }
            }
        }

        public void RemoveViewports(int[] indexes)
        {
            foreach (var index in indexes)
            {
                if (this.indexedViewports.ContainsKey(index))
                {
                    var viewport = this.indexedViewports[index];
                    renderer.RemoveViewport((viewport as AcousticThreeDViewport).Viewport);
                    this.indexedViewports.Remove(index);
                }
            }
        }

        public void UpdateViewports(int[] indexes, ImageAreaViewportDTO[] viewports)
        {
            int i = 0;
            foreach (var index in indexes)
            {
                var viewport = viewports[i];

                if (this.indexedViewports.ContainsKey(index))
                {
                    var obj = this.indexedViewports[index];

                    switch (obj)
                    {
                        case AcousticThreeDViewport threeD:
                            threeD.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                            threeD.SetVisibility(viewport.IsVisible);
                            break;
                        case AcousticTwoDViewport twoD:
                            twoD.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                            break;
                    }
                }

                i++;
            }
        }

        public void SetViewports(int[] indexes, ImageAreaViewportWithContextDTO[] viewports)
        {
            int i = 0;
            foreach (var index in indexes)
            {
                var viewport = viewports[i];

                if (this.indexedViewports.ContainsKey(index))
                {
                    var existing = this.indexedViewports[index];
                    if (viewport.ViewportType == ViewportType.Review && existing is AcousticThreeDViewport threeDViewport)
                    {
                        threeDViewport.SetArchivedDataSource(viewport.Context as IArchivedDataSource);
                        threeDViewport.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                        threeDViewport.SetVisibility(true);
                    }
                    else if (viewport.ViewportType == ViewportType.Acquisition && existing is AcousticTwoDViewport twoD)
                    {
                        twoD.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                    }
                }
                else
                {
                    if (viewport.ViewportType == ViewportType.Review)
                    {
                        var threeDViewport = new AcousticThreeDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                        this.acousticThreeDViewports.Add(threeDViewport);
                        this.indexedViewports[index] = threeDViewport;
                        renderer.AddViewport(threeDViewport.Viewport);
                        if (viewport.Context is IArchivedDataSource archived)
                        {
                            threeDViewport.SetArchivedDataSource(archived);
                        }
                    }
                    else
                    {
                        var twoD = new AcousticTwoDViewport(systemTime, renderedWindow, mouseCallback, renderer, creator, viewport.X, viewport.Y, viewport.Width, viewport.Height);
                        this.acousticTwoDViewport.Add(twoD);
                        this.indexedViewports[index] = twoD;
                        renderer.AddViewport(twoD.Viewport);
                    }
                }

                i++;
            }
        }


        public void UpdateViewport(int index, ImageAreaViewportDTO viewport)
        {
            // Forward single update to multi-update implementation
            UpdateViewports(new int[] { index }, new ImageAreaViewportDTO[] { viewport });
        }
    }


}

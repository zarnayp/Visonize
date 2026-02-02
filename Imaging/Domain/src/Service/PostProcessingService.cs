using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Entities.Scene;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.Player;
using Visonize.UsImaging.Domain.SceneObjects;
using Visonize.UsImaging.Domain.Service.Framework;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Domain.ValueObjects;
using PragmaticScene.SceneControl;
using PragmaticScene.SceneInterfaces;

namespace Visonize.UsImaging.Domain.Service
{
    public class PostProcessingService : IPostProcessingService
    {
        private readonly ISystemTime systemTime;
        private readonly IRenderer renderer;
        private ImageViewArea imageViewArea;

        public PostProcessingService(ISystemTime systemTime, ICreator creator, PragmaticScene.Scene.Window renderingWindow, IMouseCallback mouseCallback, IRenderer renderer)
        {
            this.systemTime = systemTime;
            this.imageViewArea = new ImageViewArea(systemTime, creator, mouseCallback, renderer.PragmaticSceneRenderer, renderingWindow); //tODO: CREATE IMAGE AREA HERE
            this.renderer = renderer;
        }

        public ImageViewArea ImageViewArea => this.imageViewArea;


        public event EventHandler<IRgbImageReference> ImageRendered;

        public void Process(IRgbImageReference targetImageReference)
        {
           // this.imageViewArea.Update();
            this.renderer.RenderWithReadback(targetImageReference); // todo: inject imageViewArea in this place?
        }

        public void ProcessRawData(RawFrameDescription rawFrameDescription, IRgbImageReference rgbImageReference)
        {
            // acousticTwoDViewport.Add(twoDViewport0);
            // acousticTwoDViewport.Add(twoDViewport0);
            this.imageViewArea.UpdateImage(rawFrameDescription.RawFrameReference, new CurvedGeometryData());
            this.renderer.RenderWithReadback(rgbImageReference); // todo: inject imageViewArea in this place?
            //var image = this.renderer.GetLatestData();

            //this.ImageRendered?.Invoke(this, image);
        }

    }
}

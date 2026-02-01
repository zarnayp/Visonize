using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.Common;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Domain.SceneObjects;

namespace DupploPulse.UsImaging.Domain.Player
{
    internal class CinePlayer : ICinePlayer
    {
        private ICine cine;
        private ISystemTime systemTime;
        private ITwoDImageSceneObject twoDImageSceneObject;

        private int currentImageIndex;

        public CinePlayer(ICine cine, ISystemTime systemTime, ITwoDImageSceneObject twoDImageSceneObject)
        {
            this.cine = cine;
            this.systemTime = systemTime;
            this.twoDImageSceneObject = twoDImageSceneObject;
        }

        public void StartPlayback()
        {
            throw new NotImplementedException();
        }

        public void StopPlayback()
        {
            throw new NotImplementedException();
        }

        public void ScrollFrame(int delta)
        {
            this.cine.ScrollByFrame(delta);
            var frame = this.cine.GetCurrentFrame();
            this.twoDImageSceneObject.UpdateImageData(frame.RawFrameReference, new CurvedGeometryData());
        }

        public void Update()
        {
            currentImageIndex++;
            if(currentImageIndex >= this.cine.NumberOfImages)
            {
                currentImageIndex = 0;
            }
            var geometryData = new CurvedGeometryData();
        //    this.twoDImageSceneObject.UpdateImageData(this.cine.GetRawImage(currentImageIndex), geometryData);
        } 
    }
}
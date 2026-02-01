using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Domain.SceneObjects;

namespace DupploPulse.UsImaging.Domain.Entities.Scene
{
    internal class TwoDPostProcessing : ITwoDImagePostProcessing
    {
        private ITwoDImageSceneObject twoDImageSceneObject;

        public TwoDPostProcessing(ITwoDImageSceneObject twoDImageSceneObject)
        {
            this.twoDImageSceneObject = twoDImageSceneObject;
        }

        public void ChangeGrayMapIndex(int change)
        {
            //this.twoDImageSceneObject.GrayMap = this.twoDImageSceneObject.GrayMap + change;
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.SceneObjects;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.SceneObjects;

namespace Visonize.UsImaging.Domain.Entities.Scene
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

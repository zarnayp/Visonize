using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Infrastracture.SceneObjects;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.Scene;
using PragmaticScene.SceneInterfaces;

namespace DupploPulse.UsImaging.Infrastructure.Renderer.Scene
{
    public class UsSceneObjectsCreator : ISceneObjectsCreator
    {
        public T? Create2DSceneObject<T>(Space2D space2D) where T : class, IRenderableSceneObject
        {
            return null;
        }

        public T? Create2DSceneObject<T>(Space2D space2D, int id) where T : class, IIdentificableSceneObject
        {
            return null;
        }

        public T? CreateSceneObject<T>() where T : class, IRenderableSceneObject
        {
            if (typeof(T) == typeof(ITwoDImageSceneObject))
            {
                return (T)(IRenderableSceneObject)new TwoDImageSceneObject();
            }
            if (typeof(T) == typeof(IThreeDImageSceneObject))
            {
                return (T)(IRenderableSceneObject)new ThreeDImageSceneObject();
            }

            return null;
        }

        public T? CreateSceneObject<T>(int id) where T : class, IIdentificableSceneObject
        {
            return null;
        }
    }
}

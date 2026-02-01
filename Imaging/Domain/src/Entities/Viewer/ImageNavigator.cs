using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Interfaces;

namespace DupploPulse.UsImaging.Domain.Entities.Viewer
{
    internal class ImageNavigator : IImageNavigator
    {
        private readonly List<UsImage> images = new ();

        public event EventHandler<UsImage> ImageAdded;

        public  void AddImage(UsImage image)
        {
            if(this.images.Contains(image))
            {
                return;
            }

            this.images.Add (image);

            ImageAdded?.Invoke(this, image);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Entities.Viewer;

namespace DupploPulse.UsImaging.Domain.Interfaces
{
    public interface IImageNavigator
    {
        event EventHandler<UsImage> ImageAdded;
    }
}

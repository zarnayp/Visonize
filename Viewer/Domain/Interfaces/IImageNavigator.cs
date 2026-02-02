using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;

namespace Visonize.Viewer.Domain.Interfaces
{
    public interface IImageNavigator
    {
        event EventHandler<UsImage> ImageAdded;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Entities.Viewer;

namespace DupploPulse.UsImaging.Domain.Interfaces
{
    public interface IImageWorkspace
    {
        event EventHandler<UsImage[]> ImagesChanged;

        event EventHandler<Layout> LayoutChanged;

        IReadOnlyList<UsImage?> Images { get; }

        void ChangeLayout(Layout layout);

        void RemoveImage(int indexe);
    }
}

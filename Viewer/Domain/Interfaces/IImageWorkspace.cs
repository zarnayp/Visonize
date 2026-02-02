using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;

namespace Visonize.Viewer.Domain.Interfaces
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

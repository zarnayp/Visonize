using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;

namespace Visonize.Viewer.Domain.Interfaces
{
    public enum Layout
    {
        Live,
        Dual,
        Review1x1,
        Review2x2

    }

    public interface IViewer
    {
        void ChangeLayout(Layout layout); // TODO: move it to ImageWorkspace?

        void AddImage(UsImage image);

        IImageNavigator ImageNavigator { get; }

        IImageWorkspace ImageWorkspace { get; }

        void LoadImageToWorkspace(UsImage image);

        void LoadImageToWorkspace(UsImage image, int workspaceLayoutId);

        void UnloadImageToWorkspace(UsImage image);

    }

}

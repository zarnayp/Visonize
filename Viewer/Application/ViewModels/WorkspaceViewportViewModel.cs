using System.ComponentModel;
using System.Windows.Input;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.Viewer.Domain.Interfaces;

namespace Visonize.UsImaging.Application.ViewModels
{
    public class WorkspaceViewportViewModel : INotifyPropertyChanged
    {
        private readonly int index;
        private IViewer? viewer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WorkspaceViewportViewModel(int index)
        {
            this.index = index;
            this.CloseCommand = new RelayCommand(Close);
        }

        public int Index => this.index;

        public ICommand CloseCommand { get; }

        public void Initialize(IViewer viewer)
        {
            this.viewer = viewer;
        }

        public void Close()
        {
            if (this.viewer == null) return;
            this.viewer.ImageWorkspace.RemoveImage(this.index);
        }
    }
}

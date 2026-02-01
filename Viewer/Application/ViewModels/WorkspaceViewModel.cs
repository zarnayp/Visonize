using System.ComponentModel;
using DupploPulse.UsImaging.Domain.Interfaces;

namespace DupploPulse.UsImaging.Application.ViewModels
{
    public class WorkspaceViewModel : INotifyPropertyChanged
    {
        private IViewer? viewer;
        private Layout layout;

        public event PropertyChangedEventHandler? PropertyChanged;

        // New properties controlling overlay visibility
        private bool viewport1Visible;
        public bool Viewport1Visible
        {
            get => viewport1Visible;
            private set
            {
                if (viewport1Visible == value) return;
                viewport1Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Viewport1Visible)));
            }
        }

        private bool viewport2Visible;
        public bool Viewport2Visible
        {
            get => viewport2Visible;
            private set
            {
                if (viewport2Visible == value) return;
                viewport2Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Viewport2Visible)));
            }
        }

        private bool viewport3Visible;
        public bool Viewport3Visible
        {
            get => viewport3Visible;
            private set
            {
                if (viewport3Visible == value) return;
                viewport3Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Viewport3Visible)));
            }
        }

        private bool viewport4Visible;
        public bool Viewport4Visible
        {
            get => viewport4Visible;
            private set
            {
                if (viewport4Visible == value) return;
                viewport4Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Viewport4Visible)));
            }
        }

        // Layout property is connected to IViewer: when set it calls viewer.ChangeLayout(...).
        // It also updates visibility booleans according to the chosen layout.
        public Layout Layout
        {
            get => this.layout;
            set
            {
                if (this.layout == value) return;
                this.layout = value;
                this.viewer?.ChangeLayout(this.layout);
                UpdateViewportVisibilitiesForLayout(this.layout);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Layout)));
            }
        }

        // IViewer is injected at runtime via Initialize (same pattern as ThumbnailsViewModel)
        public void Initialize(IViewer viewer)
        {
            this.viewer = viewer;
            this.viewer.ImageWorkspace.LayoutChanged += ImageWorkspace_LayoutChanged;
            // initialize viewport viewmodels with viewer
            this.Viewport0.Initialize(viewer);
            this.Viewport1.Initialize(viewer);
            this.Viewport2.Initialize(viewer);
            this.Viewport3.Initialize(viewer);
        }

        // Called by UI when user clicks close on a viewport overlay
        public void RemoveImageAt(int index)
        {
            if (this.viewer == null) return;
            this.viewer.ImageWorkspace.RemoveImage(index);
        }

        // Per-viewport viewmodels
        public WorkspaceViewportViewModel Viewport0 { get; } = new WorkspaceViewportViewModel(0);
        public WorkspaceViewportViewModel Viewport1 { get; } = new WorkspaceViewportViewModel(1);
        public WorkspaceViewportViewModel Viewport2 { get; } = new WorkspaceViewportViewModel(2);
        public WorkspaceViewportViewModel Viewport3 { get; } = new WorkspaceViewportViewModel(3);

        private void ImageWorkspace_LayoutChanged(object? sender, Layout e)
        {
            this.layout = e;
            UpdateViewportVisibilitiesForLayout(this.layout);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Layout)));
        }

        private void UpdateViewportVisibilitiesForLayout(Layout layout)
        {
            // Default: all false
            Viewport1Visible = false;
            Viewport2Visible = false;
            Viewport3Visible = false;
            Viewport4Visible = false;

            switch (layout)
            {
                case Layout.Live:
                    // Live -> single overlay (viewport1)
                    Viewport1Visible = true;
                    break;
                case Layout.Dual:
                    // Dual -> left and right
                    Viewport1Visible = true;
                    Viewport2Visible = true;
                    break;
                case Layout.Review1x1:
                    // 1x1 review -> single overlay (use viewport1)
                    Viewport1Visible = true;
                    break;
                case Layout.Review2x2:
                    // 2x2 -> all four
                    Viewport1Visible = true;
                    Viewport2Visible = true;
                    Viewport3Visible = true;
                    Viewport4Visible = true;
                    break;
            }
        }
    }
}
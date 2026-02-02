using System;
using Visonize.UsImaging.Application.Infrastructure;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.Viewer.Domain.Interfaces;

namespace Visonize.UsImaging.Application.ViewModels
{
    public class ViewerViewModel
    {
        IViewer? viewer;

        private Layout currentLayout;

        private ThumbnailsViewModel thumbnailsViewModel;
        private WorkspaceViewModel workSpaceViewModel;
        private readonly Lazy<IDicomRepository> lazyDicomRepository;

        private IDicomRepository dicomRepository => this.lazyDicomRepository.Value;

        //private Lazy<ThumbnailsViewModel> thumbnailsViewModel = new (() => new ThumbnailsViewModel(this.dicomFileUtils));
            
        public ViewerViewModel()
        {
        }

        public ViewerViewModel(Lazy<IDicomRepository> lazyDicomRepository, Lazy<IDicomRepositoryFileParser> lazyDicomFileParser)
        {
            this.lazyDicomRepository = lazyDicomRepository;
            this.thumbnailsViewModel = new ThumbnailsViewModel(lazyDicomFileParser);

            // instantiate WorkSpaceViewModel (no constructor injection required)
            this.workSpaceViewModel = new WorkspaceViewModel();
        }

        public ThumbnailsViewModel Thumbnails => this.thumbnailsViewModel;

        // Expose WorkSpaceViewModel
        public WorkspaceViewModel WorkSpace => this.workSpaceViewModel;

        public void Initialize(IViewer viewer)
        {
            this.viewer = viewer;
            this.thumbnailsViewModel.Initialize(viewer);

            // initialize the WorkSpaceViewModel with the IViewer instance
            this.workSpaceViewModel.Initialize(viewer);
        }

        public Layout CurrentLayout
        {
            get
            {
                return this.currentLayout;
            }
            set
            {
                this.currentLayout = value;
                this.viewer?.ChangeLayout(this.currentLayout);
            }
        }

        public void LoadFile(string[] filePath)
        {
            foreach (var path in filePath)
            {
                var image = this.dicomRepository.AddImage(path);
                this.viewer?.AddImage(image);
            }

        }
    }
}

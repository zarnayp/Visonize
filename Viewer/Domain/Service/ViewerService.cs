using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;
using Visonize.Viewer.Domain.Interfaces;
using Visonize.Viewer.Domain.Infrastructure;
using Visonize.Viewer.Domain.Service.Infrastructure;
using Visonize.UsImaging.Domain.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Visonize.UsImaging.Domain.Service;

namespace Visonize.Viewer.Domain.Service
{
    public class ImagingViewAreaAdapter : Visonize.Viewer.Domain.Infrastructure.IImageViewArea
    {
        private readonly Visonize.UsImaging.Domain.Interfaces.IImageViewArea imageViewArea;
        private readonly IArchivedDataSourceFactory archivedDataSourceFactory;

        public ImagingViewAreaAdapter(Visonize.UsImaging.Domain.Interfaces.IImageViewArea imageViewArea, IArchivedDataSourceFactory archivedDataSourceFactory)
        {
            this.archivedDataSourceFactory = archivedDataSourceFactory;
            this.imageViewArea = imageViewArea;
        }

        public void RemoveViewports(int[] indexes)
        {
            this.imageViewArea.RemoveViewports(indexes);
        }

        public async void SetViewports(int[] indexes, Visonize.Viewer.Domain.Infrastructure.ViewportWithImageDTO[] viewports)
        {
            var dataSource = await this.archivedDataSourceFactory.CreateArchivedDataSource(viewports[0].Image);

            // map to domain interface DTO with context
            var viewport = new Visonize.UsImaging.Domain.Interfaces.ImageAreaViewportWithContextDTO
            {
                X = viewports[0].X,
                Y = viewports[0].Y,
                Width = viewports[0].Width,
                Height = viewports[0].Height,
                ViewportType = Visonize.UsImaging.Domain.Interfaces.ViewportType.Review,
                Context = dataSource
            };

            this.imageViewArea.SetViewports(indexes, new Visonize.UsImaging.Domain.Interfaces.ImageAreaViewportWithContextDTO[] { viewport });
        }

        public void UpdateViewports(int[] indexes, Visonize.Viewer.Domain.Infrastructure.ViewportDTO[] viewports)
        {
            // map dimension-only DTOs to ImageAreaViewportDTO and call UpdateViewports
            var mapped = viewports.Select(v => new Visonize.UsImaging.Domain.Interfaces.ImageAreaViewportDTO { X = v.X, Y = v.Y, Width = v.Width, Height = v.Height, IsVisible = v.IsVisible }).ToArray();
            this.imageViewArea.UpdateViewports(indexes, mapped);
        }
    }

    public class ViewerService : IViewer
    {
        private readonly IPostProcessingService postProcessingService;
        private readonly Visonize.UsImaging.Domain.Interfaces.IImageViewArea imageViewArea;
        private readonly IImaging imaging;
        private readonly IArchivedDataSourceFactory archivedDataSourceFactory;

        private readonly Lazy<ImageNavigator> lazyNavigator = new(() => new ImageNavigator());

        private ImageNavigator imageNavigator => lazyNavigator.Value;

        public IImageNavigator ImageNavigator => lazyNavigator.Value;

        public IImageWorkspace ImageWorkspace => this.imageWorkspace;

        private readonly ImageWorkspace imageWorkspace;

        public ViewerService(IImaging imaging, IPostProcessingService postProcessingService , Visonize.UsImaging.Domain.Interfaces.IImageViewArea imageViewArea, IArchivedDataSourceFactory archivedDataSourceFactory)
        {
            this.imaging = imaging;
            this.postProcessingService = postProcessingService;
            this.imageViewArea = imageViewArea;
            this.archivedDataSourceFactory = archivedDataSourceFactory;
            this.imageWorkspace = new ImageWorkspace(new ImagingViewAreaAdapter(this.imageViewArea,this.archivedDataSourceFactory));
        }

        private void FreezeIfLive()
        {
            this.imaging.Freeze();
        }

        public void ChangeLayout(Layout layout) // todo: maybe move it to imageworkspace
        {
            this.imageWorkspace.ChangeLayout(layout);

        }



        public void AddImage(UsImage image)
        {
            this.imageNavigator.AddImage(image);
        }

        public async Task LoadImageToWorkspace(UsImage image)
        {
            this.imageWorkspace.AddImage(image);
        }

        public void LoadImageToWorkspace(UsImage image, int workspaceLayoutId)
        {
            this.imageWorkspace.RemoveImage(image);
        }

        async void IViewer.LoadImageToWorkspace(UsImage image)
        {
            this.imageWorkspace.AddImage(image);

            //var dataSource = await this.archivedDataSourceFactory.CreateArchivedDataSource(image);

            //ImageAreaViewport viewport = new ImageAreaViewport
            //{
            //    X = 0f,
            //    Y = 0f,
            //    Width = 1f,
            //    Height = 1f,
            //    ViewportType = ViewportType.Review,
            //    Context = dataSource
            //};

            //this.imageViewArea.UpdateViewport(0, viewport);
        }

        public void UnloadImageToWorkspace(UsImage image)
        {
            this.imageWorkspace.RemoveImage(image);
        }
    }
}

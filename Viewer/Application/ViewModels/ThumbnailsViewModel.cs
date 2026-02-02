using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Visonize.UsImaging.Application.Infrastructure;
using Visonize.Viewer.Domain.Entities;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.Viewer.Domain.Interfaces;

namespace Visonize.UsImaging.Application.ViewModels
{
    public class ThumbnailsViewModel : INotifyPropertyChanged
    {
        private IViewer? viewer;

        private readonly Lazy<IDicomRepositoryFileParser> lazyDicomParser;
        private IDicomRepositoryFileParser dicomParser => lazyDicomParser.Value;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Thumbnail> Thumbnails { get; } = new ObservableCollection<Thumbnail>();


        public ThumbnailsViewModel(Lazy<IDicomRepositoryFileParser> lazyDicomParser)
        {
            this.lazyDicomParser = lazyDicomParser;
        }

        public void Initialize(IViewer viewer)
        {
            this.viewer = viewer;

            this.viewer.ImageNavigator.ImageAdded += (s, e) => ImageAdded(s, e);            
            this.viewer.ImageWorkspace.ImagesChanged += (s, e) => WorkspaceImagesChanged(s, e);
        }

        private void WorkspaceImagesChanged(object s, UsImage[] e)
        {
            foreach (var thumbnail in this.Thumbnails)
            {
                if (e.Contains(thumbnail.UsImage))
                {
                    thumbnail.Selected = true;
                }
                else
                {
                    thumbnail.Selected = false;
                }
            }
        }

        private async void ImageAdded(object sender, UsImage image)
        {
            var thumbnail = new Thumbnail(image);
            this.Thumbnails.Add(thumbnail);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnails)));

            thumbnail.ImageData = await this.dicomParser.GetThumbnailAsync(image.Id);

        }

        public void SelectImage(UsImage image)
        {
            viewer?.LoadImageToWorkspace(image);
        }

        public void UnselectImage(UsImage image)
        {
            viewer?.UnloadImageToWorkspace(image);
        }

    }
}

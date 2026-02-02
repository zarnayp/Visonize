using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Application.Infrastructure;
using Visonize.Viewer.Domain.Entities;

namespace DupploPulse.UsImaging.Application.ViewModels
{
    public enum ImageEncoding
    {
        RawRgb,
        RawYbr,
        RawMono,
        CompressedJpeg,
        CompressedJpegLs,
        CompressedJpeg2000,
        CompressedRle,
        Unknown

    }

    public struct ThumbnailImage
    {
        public byte[]? ImageData;
        public ImageEncoding ImageEncoding;
        public int Width;
        public int Height;
    }

    public class Thumbnail : INotifyPropertyChanged
    {
        private ThumbnailImage imageData;
        private bool selected;

        public Thumbnail(UsImage usImage)
        {
            this.imageData = new ThumbnailImage
            {
                ImageData = null,
                ImageEncoding = ImageEncoding.Unknown,
                Width = 0,
                Height = 0
            };
            this.selected = false;
            this.UsImage = usImage;
        }

        public UsImage UsImage { get; private set; }

        // RGB image data (200x150) as contiguous bytes (R,G,B)
        public ThumbnailImage ImageData
        {
            get => imageData;
            set
            {
                imageData = value;
                OnPropertyChanged(nameof(ImageData));
            }
        }


        public bool Selected
        {
            get => this.selected;
            set
            {
                this.selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}



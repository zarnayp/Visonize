using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Application.Infrastructure;

using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.SceneObjects;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Infrastructure.Dicom;
using Visonize.Viewer.Domain.Entities;
using Visonize.Viewer.Domain.Service.Infrastructure;

namespace DupploPulse.UsImaging.Infrastracture.ImageSource
{
    public class ArchivedDataSourceFactory : IArchivedDataSourceFactory
    {
        private readonly Lazy<DicomRepository> dicomRepositoryLazy;
        private DicomRepository dicomRepository => dicomRepositoryLazy.Value;

        private readonly DicomFileUtils dicomFileUtils;

        public ArchivedDataSourceFactory(Lazy<DicomRepository> dicomRepositoryLazy)
        {
            this.dicomRepositoryLazy = dicomRepositoryLazy;
            this.dicomFileUtils = new DicomFileUtils();
        }

        public Task<IArchivedDataSource?> CreateArchivedDataSource(UsImage image)
        {
            return Task.Run(() =>
            {
                return CreateFromDicom(image);
            });
        }

        private IArchivedDataSource? CreateFromDicom(UsImage image)
        {
            try
            {
                string path = dicomRepository.GetFilePath(image.Id);

                var info = this.dicomFileUtils.GetCartezian3DImageDataInfo(path);

                var archivedDataSource = new DicomFileCine(1);

                VolumeCartezianGeometry geometry = new VolumeCartezianGeometry();

                geometry.Width = info.width;
                geometry.Height = info.height;
                geometry.Depth = info.depth;

                RawFrameMetaData metaData = new RawFrameMetaData();
                metaData.GeometryData = geometry;

                archivedDataSource.SetFrame(0, info.firstFrameImageData, metaData);

                return archivedDataSource;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

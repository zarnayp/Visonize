using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Application.Infrastructure;
using Visonize.UsImaging.Application.ViewModels;
using FellowOakDicom;
using static System.Net.Mime.MediaTypeNames;

namespace Visonize.UsImaging.Infrastructure.Dicom
{
    public class DicomRepositoryFileParser : IDicomRepositoryFileParser
    {
        Lazy<DicomRepository> lazyDicomRepository;

        DicomFileUtils fileUtils;

        DicomRepository dicomRepository => lazyDicomRepository.Value; 

        public DicomRepositoryFileParser(Lazy<DicomRepository> lazyDicomRepository)
        {
            this.lazyDicomRepository = lazyDicomRepository;
            this.fileUtils = new DicomFileUtils();
        }

        public Task<ThumbnailImage> GetThumbnailAsync(string imageId)
        {
            var filePath = this.dicomRepository.GetFilePath(imageId);

            return this.fileUtils.GetThumbnailAsync(filePath);
        }

        public void GetImageData(string imageId)
        {
            var filePath = this.dicomRepository.GetFilePath(imageId);

            var dicomFile = DicomFile.Open(filePath);
            var dataset = dicomFile.Dataset;

            // string creator = dataset.GetPrivateCreator(0x0029, 0x0040);
            //  Console.WriteLine($"Private creator for (0029,0040): {creator}");

            // Now build the private tag using the creator
            var privateTag = DicomTag.Parse("0029,1040:SIEMENS MEDCOM HEADER");


            var targetTag = DicomTag.Parse("0029,1044:SIEMENS MEDCOM HEADER");

            foreach (var item in dataset)
            {
                if (item.Tag == privateTag && item is DicomSequence sequence)
                {
                    var seqItem = sequence.Items[6];
                    {
                        if (seqItem.Contains(targetTag))
                        {
                            var element = seqItem.GetDicomItem<DicomElement>(targetTag);
                            if (element != null)
                            {
                                byte[] data = element.Buffer.Data;
                            }
                        }
                    }
                }
            }
        }
    }
}

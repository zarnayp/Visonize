using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Visonize.UsImaging.Application.Infrastructure;
using Visonize.UsImaging.Application.ViewModels;
using FellowOakDicom;
using FellowOakDicom.Imaging;

namespace Visonize.UsImaging.Infrastructure.Dicom
{
    public class DicomFileUtilsFactory : IDicomFileUtilsrFactory
    {
        public IDicomFileUtils CreateDicomFileUtils()
        {
            return new DicomFileUtils();
        }
    }

    public struct Cartezian3DImageDataInfo
    {
        public int width;
        public int height;
        public int depth;
        public int numOfFrames;
        public byte[]? firstFrameImageData;

    }

    public class DicomFileUtils : IDicomFileUtils
    {
        public Cartezian3DImageDataInfo GetCartezian3DImageDataInfo(string file)
        {
            var info = new Cartezian3DImageDataInfo
            {
                width = 0,
                height = 0,
                depth = 0,
                numOfFrames = 1 // hardcoded for now
            };

            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                return info;

            try
            {
                var dicomFile = DicomFile.Open(file, FileReadOption.Default);
                var ds = dicomFile.Dataset;

                var tagWidth = new DicomTag(0x0039, 0x1041, "SIEMENS MED SMS USG S2000 3D VOLUME");
                var tagHeight = new DicomTag(0x0039, 0x1042, "SIEMENS MED SMS USG S2000 3D VOLUME");
                var tagDepth = new DicomTag(0x0039, 0x1043, "SIEMENS MED SMS USG S2000 3D VOLUME");

                if (ds.TryGetSingleValue(tagWidth, out string? widthValue) && int.TryParse(widthValue, out int w))
                    info.width = w;

                if (ds.TryGetSingleValue(tagHeight, out string? heightValue) && int.TryParse(heightValue, out int h))
                    info.height = h;

                if (ds.TryGetSingleValue(tagDepth, out string? depthValue) && int.TryParse(depthValue, out int d))
                    info.depth = d;


                var privateTag = DicomTag.Parse("0029,1040:SIEMENS MEDCOM HEADER");
                var seqTag = new DicomTag(0x0029, 0x1040); // sequence
                var targetTag = DicomTag.Parse("0029,1044:SIEMENS MEDCOM HEADER");
                var itemTitleTag = DicomTag.Parse("0029,1042:SIEMENS MEDCOM HEADER");


                var sequence = ds.GetSequence(privateTag);

                foreach (var seqItem in sequence.Items)
                {
                    var itemTitle = seqItem.GetDicomItem<DicomElement>(itemTitleTag);

                    if(itemTitle != null && itemTitle.Get<string>() == "Cartesian Data")
                    { 
                        if (seqItem.Contains(targetTag))
                        {
                            var element = seqItem.GetDicomItem<DicomElement>(targetTag);
                            if (element != null)
                            {
                                info.firstFrameImageData = element.Buffer.Data;
                            }
                        }
                    }
                }
            }
            catch
            {
                // On any error return default-initialized info (width/height/depth = 0, numOfFrames = 1)
            }

            return info;
        }

        public Task<ThumbnailImage> GetThumbnailAsync(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                return null;

            return Task.Run(() =>
            {
                try
                {
                    ThumbnailImage thumbnailImage = new ThumbnailImage();
                    var dicomFile = DicomFile.Open(file, FileReadOption.Default);

                    var encoding = GetImageEncoding(dicomFile.Dataset);
                    var pixelData = DicomPixelData.Create(dicomFile.Dataset);
                    int width = pixelData.Width;
                    int height = pixelData.Height;

                    thumbnailImage.ImageData = pixelData.GetFrame(0).Data;
                    thumbnailImage.ImageEncoding = encoding;
                    
                    thumbnailImage.Width = width;
                    thumbnailImage.Height = height;

                    return thumbnailImage;
                }
                catch
                {
                    return new ThumbnailImage();
                }
            });
        }

         

        public static ImageEncoding GetImageEncoding(DicomDataset ds)
        {
            var ts = ds.InternalTransferSyntax;

            // 1. Compressed?
            if (ts.IsEncapsulated)
            {
                var uid = ts.UID.UID;

                if (uid.StartsWith("1.2.840.10008.1.2.4.5"))   // JPEG Baseline / Extended / Lossless
                    return ImageEncoding.CompressedJpeg;

                if (uid.StartsWith("1.2.840.10008.1.2.4.8"))   // JPEG-LS
                    return ImageEncoding.CompressedJpegLs;

                if (uid.StartsWith("1.2.840.10008.1.2.4.9"))   // JPEG2000
                    return ImageEncoding.CompressedJpeg2000;

                if (uid == "1.2.840.10008.1.2.5")              // RLE
                    return ImageEncoding.CompressedRle;

                return ImageEncoding.Unknown;
            }

            // 2. Uncompressed check Photometric Interpretation
            var pi = ds.GetSingleValueOrDefault(DicomTag.PhotometricInterpretation, "");

            return pi switch
            {
                "RGB" => ImageEncoding.RawRgb,
                "YBR_FULL" => ImageEncoding.RawYbr,
                "YBR_FULL_422" => ImageEncoding.RawYbr,
                "MONOCHROME1" => ImageEncoding.RawMono,
                "MONOCHROME2" => ImageEncoding.RawMono,
                _ => ImageEncoding.Unknown
            };
        }

    }
}

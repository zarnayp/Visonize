using System;
using System.Collections.Concurrent;
using FellowOakDicom;
using DupploPulse.UsImaging.Application.Infrastructure;
using DupploPulse.UsImaging.Domain.Entities.Viewer;

namespace DupploPulse.UsImaging.Infrastructure.Dicom;

public class DicomRepository : IDicomRepository
{
    struct DicomFileInfo
    {
        public DicomFileInfo(string path, UsImage usImage)
        {
            FilePath = path;

            DomainImage = usImage;
        }

        public string FilePath { get; private set; }

        public UsImage DomainImage { get; private set; }
    }

    // Stores mapping from image id (SOP Instance UID or fallback GUID) -> original file path
    private readonly Dictionary<string, DicomFileInfo> dicomFilesInfo = new();

    public UsImage AddImage(string file)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));

        string id;
        try
        {
            var dicomFile = DicomFile.Open(file);
            var dataset = dicomFile.Dataset;

            // Try to read SOP Instance UID from the DICOM dataset
            if (!dataset.TryGetSingleValue(DicomTag.SOPInstanceUID, out string? sopUid) || string.IsNullOrWhiteSpace(sopUid))
            {
                // fallback if tag missing or empty
                id = Guid.NewGuid().ToString();
            }
            else
            {
                id = sopUid;
            }
        }
        catch
        {
            // On any parse/open error fall back to a GUID
            id = Guid.NewGuid().ToString();
        }

        if (!dicomFilesInfo.ContainsKey(id)) // ignore new file with same id
        {
            dicomFilesInfo[id] = new DicomFileInfo(file, new UsImage(id));
        }
 
        return dicomFilesInfo[id].DomainImage;
    }

    // Helper for future lookup by id
    public string GetFilePath(string id)
    {
        if (dicomFilesInfo.TryGetValue(id, out var info))
        {
            return info.FilePath;
        }
        return string.Empty;
    }
}

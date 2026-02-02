using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Application.ViewModels;

namespace DupploPulse.UsImaging.Application.Infrastructure
{
    public interface IDicomFileUtilsrFactory
    {
        IDicomFileUtils CreateDicomFileUtils();
    }

    public interface IDicomFileUtils
    {
         Task<ThumbnailImage> GetThumbnailAsync(string file);
    }
}

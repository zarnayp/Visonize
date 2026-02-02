using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Application.ViewModels;

namespace Visonize.UsImaging.Application.Infrastructure
{
    public interface IDicomRepositoryFileParser
    {
        Task<ThumbnailImage> GetThumbnailAsync(string imageId);
    }
}

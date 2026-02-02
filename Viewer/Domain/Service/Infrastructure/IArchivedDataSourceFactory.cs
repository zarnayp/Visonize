using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;
using Visonize.UsImaging.Domain.Player;

namespace Visonize.Viewer.Domain.Service.Infrastructure
{
    public interface IArchivedDataSourceFactory
    {
        Task<IArchivedDataSource?> CreateArchivedDataSource(UsImage image);
    }
}

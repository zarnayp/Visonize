using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Entities.Viewer;
using DupploPulse.UsImaging.Domain.Player;

namespace DupploPulse.UsImaging.Domain.Service.Infrastructure
{
    public interface IArchivedDataSourceFactory
    {
        Task<IArchivedDataSource?> CreateArchivedDataSource(UsImage image);
    }
}

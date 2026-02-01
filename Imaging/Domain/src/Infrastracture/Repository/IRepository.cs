using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Domain.Infrastracture.Repository
{
    internal interface IRepository
    {
        event EventHandler RepositoryChanged;
    }
}

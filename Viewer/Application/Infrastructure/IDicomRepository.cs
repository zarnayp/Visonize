using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;

namespace Visonize.UsImaging.Application.Infrastructure
{
    public interface IDicomRepository
    {
        UsImage AddImage(string file);

    }
}

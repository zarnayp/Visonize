using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visonize.UsImaging.Domain.Infrastracture.Common
{
    public interface ISystemTime
    {
        long GetMicrosecondTimestamp();
    }
}

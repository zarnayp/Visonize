using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.ValueObjects;

namespace Visonize.UsImaging.Domain.Player
{
    public interface ILiveDataSource
    {
        event EventHandler<RFFrameDescription> NewRFFrame;
    }
}
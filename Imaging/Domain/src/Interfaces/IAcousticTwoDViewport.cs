using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Domain.Interfaces
{
    public interface IAcousticTwoDViewport
    {
        ICinePlayer? CinePlayer { get; }

        ITwoDImagePostProcessing PostProcessing { get; }

    }
}

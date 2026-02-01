using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Player;

namespace DupploPulse.UsImaging.Domain.Interfaces
{
    public interface IImaging
    {
        IImageViewArea ImageViewArea { get; }

        void Freeze();

        void Unfreeze();


        // void HalfFreeze(); // Dual

        //void SetCineSources(Dictionary<int,ICineSource> cineSources);

        //void RemoveCineSources();
    }
}

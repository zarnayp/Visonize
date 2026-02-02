using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visonize.UsImaging.Domain.Interfaces
{
    public interface ICinePlayer
    {
        void StartPlayback();

        void StopPlayback();

        void ScrollFrame(int delta);
    }
}

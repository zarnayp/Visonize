using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Domain.Service.Framework
{
    internal interface IDispatcher
    {
        void BeginInvoke(Action action);
    }
}

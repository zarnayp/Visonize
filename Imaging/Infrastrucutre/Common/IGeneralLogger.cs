using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Infrastructure.Common
{
    public interface IGeneralLogger
    {
        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}

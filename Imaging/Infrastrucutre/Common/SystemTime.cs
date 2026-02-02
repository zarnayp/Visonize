using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;

namespace Visonize.UsImaging.Infrastructure.Common
{
    public class SystemTime : ISystemTime
    {

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private static readonly long _frequency;

        static SystemTime()
        {
            if (!QueryPerformanceFrequency(out _frequency))
                throw new InvalidOperationException("High-resolution performance counter not supported.");
        }

        public long GetMicrosecondTimestamp()
        {
            QueryPerformanceCounter(out long ticks);
            return (long)(BigInteger.Multiply(ticks, 1_000_000) / _frequency);

        }
    }
}

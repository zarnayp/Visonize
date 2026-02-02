using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.ValueObjects;

namespace Visonize.UsImaging.Domain.Player
{
    public interface ICine : IArchivedDataSource  // TODO: ICine interface is needed to push data which may be done by another bounded context context so it does not belong to player namespace
    {
        IRawFrameReference GetTargetFrameBuffer();

        void PushedTargetFrame(RawFrameMetaData metaData);

        int NumberOfImages { get; }
    }

    // Source of raw frames which are archived in cine e.g.
    public interface IArchivedDataSource
    {
        RawFrameDescription GetCurrentFrame();

        void ScrollByFrame(int numOfFrames);

        void ScrollByTime(int microseconds);
    }
}
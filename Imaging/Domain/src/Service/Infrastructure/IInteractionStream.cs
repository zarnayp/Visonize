using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visonize.UsImaging.Domain.Service.Infrastructure
{
    public enum MouseButton
    {
        None,
        Left,
        Right,
        Middle,
    }

    public class MouseEventData
    {
        public int X { get; set; }         // X coordinate in pixels
        public int Y { get; set; }         // Y coordinate in pixels
        public MouseButton Button { get; set; }
        public bool IsButtonDown { get; set; }
        public int WheelDelta { get; set; } // For scroll events
    }

    public interface IInteractionStream
    {
        event EventHandler<MouseEventData> MouseMove;

        event EventHandler<MouseEventData> MouseDown;

        event EventHandler<MouseEventData> MouseUp;
    }
}

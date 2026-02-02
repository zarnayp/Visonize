using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Infrastructure.Stream;
using PragmaticScene.SceneControl;

namespace Visonize.UsImaging.Infrastracture.CompositionRoot
{
    internal class MouseCallbackAdapter : IMouseCallback
    {
        public event EventHandler<MouseEventArgs> OnMouseMove;
        public event EventHandler<MouseEventArgs> OnMouseDown;
        public event EventHandler<MouseEventArgs> OnMouseUp;

        public MouseCallbackAdapter(GrpcInteractionServer interactionServer)
        {
            interactionServer.MouseUp += InteractionServer_OnMouseUp;
            interactionServer.MouseDown += InteractionServer_MouseDown;
            interactionServer.MouseMove += InteractionServer_MouseMove;
        }

        private void InteractionServer_MouseMove(object? sender, MouseEventData e)
        {
            // Map pressed button if provided, otherwise None
            MouseButtons mouseButtons = MouseButtons.None;

            switch (e.Button)
            {
                case MouseButton.Left:
                    mouseButtons = MouseButtons.Left;
                    break;
                case MouseButton.Right:
                    mouseButtons = MouseButtons.Right;
                    break;
                case MouseButton.Middle:
                    mouseButtons = MouseButtons.Middle;
                    break;
                default:
                    mouseButtons = MouseButtons.None;
                    break;
            }

            MouseEventArgs mouseEventArgs = new MouseEventArgs(mouseButtons, 1, e.X, e.Y, 0);
            this.OnMouseMove?.Invoke(this, mouseEventArgs);
        }

        private void InteractionServer_MouseDown(object? sender, Domain.Service.Infrastructure.MouseEventData e)
        {
            MouseButtons mouseButtons = MouseButtons.None;

            switch(e.Button) // support all buttons
            {
                case MouseButton.Left:
                    mouseButtons = MouseButtons.Left;
                    break;
                case MouseButton.Right:
                    mouseButtons = MouseButtons.Right;
                    break;
                case MouseButton.Middle:
                    mouseButtons = MouseButtons.Middle;
                    break;
                default:
                    mouseButtons = MouseButtons.None;
                    break;
            }

            MouseEventArgs mouseEventArgs = new MouseEventArgs(mouseButtons, 1, e.X, e.Y, 0);
            this.OnMouseDown?.Invoke(this, mouseEventArgs);
        }

        private void InteractionServer_OnMouseUp(object? sender, Domain.Service.Infrastructure.MouseEventData e)
        {
            MouseButtons mouseButtons = MouseButtons.None;

            switch (e.Button)
            {
                case MouseButton.Left:
                    mouseButtons = MouseButtons.Left;
                    break;
                case MouseButton.Right:
                    mouseButtons = MouseButtons.Right;
                    break;
                case MouseButton.Middle:
                    mouseButtons = MouseButtons.Middle;
                    break;
                default:
                    mouseButtons = MouseButtons.None;
                    break;
            }

            MouseEventArgs mouseEventArgs = new MouseEventArgs(mouseButtons, 1, e.X, e.Y, 0);
            this.OnMouseUp?.Invoke(this, mouseEventArgs);
        }
    }
}

using System.Collections.Generic;

namespace DupploPulse.UsImaging.Domain.Interfaces
{
    public enum ViewportType
    {
        Acquisition,
        Review
    }

    // Dimension-only DTO for updating existing viewport geometry
    public struct ImageAreaViewportDTO
    {
        public float Width;
        public float Height;
        public float X;
        public float Y;
        public bool IsVisible;
    }

    // DTO with context used when creating viewports with initial image/context
    public struct ImageAreaViewportWithContextDTO
    {
        public float Width;
        public float Height;
        public float X;
        public float Y;

        public ViewportType ViewportType;

        public object Context; // TODO: consider strong-typing this
    }

    public interface IImageViewArea
    {
        void MouseMove(float x, float y);

        void MouseDown(float x, float y);

        void MouseUp(float x, float y);

        IList<IAcousticTwoDViewport> TwoDAcousticViewports { get; }

        // TODO: below methods suppose to go another interface. Separaet one for UI and the one for another bounded context. 

        void SetViewports(int[] indexes, ImageAreaViewportWithContextDTO[] viewports);

        void UpdateViewport(int index, ImageAreaViewportDTO viewport);

        void UpdateViewports(int[] indexes, ImageAreaViewportDTO[] viewports);

        void RemoveViewports(int[] indexes);
    }
}
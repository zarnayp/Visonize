using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Standalone
{
    public partial class OverlayWindow : Window
    {
        private Window _ownerWindow;
        private FrameworkElement _targetElement;
        private HwndSource? _hwndSource;

        // Win32 constants and interop for making the window click-through
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        public OverlayWindow()
        {
            InitializeComponent();

            Loaded += OverlayWindow_Loaded;
            Unloaded += OverlayWindow_Unloaded;

            // Do not steal activation from main window
            ShowActivated = false;
        }

        private void OverlayWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _ownerWindow = Application.Current?.MainWindow ?? throw new InvalidOperationException("No MainWindow available.");
            Owner = _ownerWindow;
            ShowInTaskbar = false;

            // Look up target element in owner by x:Name
            _targetElement = _ownerWindow.FindName("imageAreaGrid") as FrameworkElement
                             ?? throw new InvalidOperationException("imageAreaGrid not found in MainWindow.");

            // Subscribe to all relevant events so overlay follows the target when it moves or resizes
            _ownerWindow.LocationChanged += OwnerWindow_PositionOrSizeChanged;
            _ownerWindow.SizeChanged += OwnerWindow_PositionOrSizeChanged;
            _ownerWindow.StateChanged += OwnerWindow_PositionOrSizeChanged;

            _targetElement.SizeChanged += TargetElement_SizeOrLayoutChanged;
            _targetElement.LayoutUpdated += TargetElement_SizeOrLayoutChanged;

            // If the overlay window already has a presentation source, hook it to make window click-through
            
            /*
            if (PresentationSource.FromVisual(this) is HwndSource src)
            {
                _hwndSource = src;
                _hwndSource.AddHook(HwndSourceHook);
                MakeWindowClickThrough();
            }
            else
            {
                // Defer setting click-through until source is initialized
                SourceInitialized += OverlayWindow_SourceInitialized;
            }
            */

            // Position once now
            UpdateOverlayPosition();
        }

        private void OverlayWindow_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (_ownerWindow != null)
            {
                _ownerWindow.LocationChanged -= OwnerWindow_PositionOrSizeChanged;
                _ownerWindow.SizeChanged -= OwnerWindow_PositionOrSizeChanged;
                _ownerWindow.StateChanged -= OwnerWindow_PositionOrSizeChanged;
            }

            if (_targetElement != null)
            {
                _targetElement.SizeChanged -= TargetElement_SizeOrLayoutChanged;
                _targetElement.LayoutUpdated -= TargetElement_SizeOrLayoutChanged;
            }

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(HwndSourceHook);
                _hwndSource = null;
            }

            SourceInitialized -= OverlayWindow_SourceInitialized;
        }

        private void OverlayWindow_SourceInitialized(object? sender, EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is HwndSource src)
            {
                _hwndSource = src;
                _hwndSource.AddHook(HwndSourceHook);
                MakeWindowClickThrough();
            }
        }

        // Make the overlay window transparent for mouse hit-testing so mouse events reach imageAreaGrid.
        // This approach avoids synthesizing mouse events and simply allows the underlying element to receive them.
        private void MakeWindowClickThrough()
        {
            if (_hwndSource == null) return;

            var hwnd = _hwndSource.Handle;
            var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
            exStyle |= WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
        }

        // Keep overlay sized and positioned exactly over the target element
        private void UpdateOverlayPosition()
        {
            if (_ownerWindow == null || _targetElement == null) return;

            // Target top-left in screen (device pixels)
            var topLeftScreen = _targetElement.PointToScreen(new Point(0, 0));

            // Convert device pixels -> WPF DIPs so we can set Window.Left/Top correctly
            var source = PresentationSource.FromVisual(_ownerWindow);
            if (source?.CompositionTarget == null) return;

            var transformFromDevice = source.CompositionTarget.TransformFromDevice;
            var topLeftDip = transformFromDevice.Transform(topLeftScreen);

            // Apply position and size in DIPs
            Left = topLeftDip.X;
            Top = topLeftDip.Y;

            // Use Actual sizes of the target element (already in DIPs)
            Width = Math.Max(0.0, _targetElement.ActualWidth);
            Height = Math.Max(0.0, _targetElement.ActualHeight);

            // Match visibility of the target so overlay hides/shows with it
            Visibility = (_targetElement.IsVisible && _ownerWindow.WindowState != WindowState.Minimized)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void OwnerWindow_PositionOrSizeChanged(object? sender, EventArgs e) => UpdateOverlayPosition();
        private void TargetElement_SizeOrLayoutChanged(object? sender, EventArgs e) => UpdateOverlayPosition();

        // Minimal HwndSource hook in case future needs arise (currently unused)
        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }
    }
}
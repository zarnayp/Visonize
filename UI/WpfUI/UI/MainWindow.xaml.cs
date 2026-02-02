using System.Diagnostics;
//sing PragmaticScene.SceneControl;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visonize.UsImaging.Application.Infrastructure;
using Visonize.UsImaging.Application.ViewModels;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.Service;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Infrastracture.Beamformer;
using Visonize.UsImaging.Infrastracture.ImageSource;
using Visonize.UsImaging.Infrastructure.Common;
using Visonize.UsImaging.Infrastructure.Dicom;
using Visonize.UsImaging.Infrastructure.Renderer;
using Visonize.UsImaging.Infrastructure.Renderer.Renderer;
using Visonize.UsImaging.Infrastructure.Renderer.Scene;
using Visonize.UsImaging.Infrastructure.Stream;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.Renderer;
using PragmaticScene.Renderer.Scene;
using Visonize.Viewer.Domain.Service;

namespace Standalone;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Overlay window instance that will track imageAreaGrid
    private OverlayWindow? _overlayWindow;

    public MainWindow()
    {
        InitializeComponent();

        this.Loaded += MainWindow_Loaded;       
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
        }
        else
        {
            try { this.DragMove(); } catch { }
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void ToggleWindowState()
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            MaxIcon.Data = System.Windows.Media.Geometry.Parse("M0,0 L10,0 L10,8 L0,8 Z");
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            MaxIcon.Data = System.Windows.Media.Geometry.Parse("M1,1 L9,1 L9,7 L1,7 Z M3,3 L7,3 L7,5 L3,5 Z");
        }
    }

    private class Lististener : TraceListener
    {
        public override void Write(string? message)
        {
            Debug.WriteLine("message");
        }

        public override void WriteLine(string? message)
        {
            Debug.WriteLine("message");
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() => StartImaging()));
    }

    private void LoadFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter = "All files (*.*)|*.*";
        dlg.Multiselect = true;
        bool? result = dlg.ShowDialog(this);
        if (result == true)
        {
            var path = dlg.FileNames;
            if (this.DataContext is Visonize.UsImaging.Application.ViewModels.ViewerViewModel vm)
            {
                vm.LoadFile(path);
            }
        }
    }

    private IImaging imaging;

    private void StartImaging()
    {
        Debug.WriteLine("Start");

        var traceSource = new TraceSource(RendererTraceSource.Common);
        traceSource.Listeners.Add(new Lististener());

        traceSource.Switch = new SourceSwitch("sourceSwitch", "All")
        {
            Level = SourceLevels.All
        };

        traceSource.TraceInformation("sssdd");

        var handle = imageAreaHost.Handle;

        var renderedWindow = new PragmaticScene.Scene.Window(1024, 720);
        Renderer renderer = new(handle, renderedWindow, PragmaticScene.RendererInterfaces.RenderingEngine.DirectX11, new () { new UsRenderersProviderCreator() });
        renderer.AddSceneObjectRenderersCreator(new SceneObjectRenderersCreator());

        //var cine = new Cine();
        //LoadMockData(cine);

       // var imageViewArea = new ImageViewArea(renderer, renderedWindow);
        

        string[] mockDataFiles = new string[10];
        for (int j = 66; j < 76; j++)
        {
            mockDataFiles[j-66] = "Assets//SavedFrame_92_18" + j + ".bin";
        }
        var mockDataLiveSource = new LiveMockFrameSource(mockDataFiles);
        mockDataLiveSource.Start();


        var cine = new Cine(100);
        var systemTime = new SystemTime();
        var imagingService = new ImagingService(mockDataLiveSource,cine, systemTime, new RgbImageCollector());
        var creator = new Creator();
        creator.AddCreator(new UsSceneObjectsCreator());
        var postProcessingService = new PostProcessingService(systemTime, creator, renderedWindow, new MouseCallbackAdapter(this.imageAreaGrid, this), new RendererAdapter(renderer)) ;
        var beamformingService = new BeamformingService(new BeamformerAdapter());

        imagingService.SetPostProcessing(postProcessingService);
        imagingService.SetBeamforming(beamformingService);
        this.imaging = imagingService;

        var dicomRepository = new Lazy<DicomRepository>(new DicomRepository());
        var lazyDicomRepository = new Lazy<IDicomRepository>(() => dicomRepository.Value);

        ArchivedDataSourceFactory archivedDataSourceFactory = new ArchivedDataSourceFactory(dicomRepository);
        ViewerService viewerService = new ViewerService(imagingService, postProcessingService, imagingService.ImageViewArea, archivedDataSourceFactory);

        Lazy<IDicomRepositoryFileParser> dicomRepositoryFileParser = new Lazy<IDicomRepositoryFileParser>(new DicomRepositoryFileParser(dicomRepository));

        var viewerViewModel = new ViewerViewModel(lazyDicomRepository, dicomRepositoryFileParser);
        viewerViewModel.Initialize(viewerService);
        this.DataContext = viewerViewModel;


        this.KeyDown += ImageAreaGrid_KeyDown;

        this.imageAreaHost.Visibility = Visibility.Visible;

        // Create and show overlay window so it starts tracking `imageAreaGrid`.
        // Set Owner so it stays tied to this MainWindow (it will close automatically with owner).
        _overlayWindow = new OverlayWindow();
        this._overlayWindow.DataContext = viewerViewModel;
        _overlayWindow.Owner = this;
        _overlayWindow.Show();
        _overlayWindow.Topmost = false;
    }

    private void ImageAreaGrid_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F:
                this.imaging.Freeze();
                break;
            case Key.U:
                this.imaging.Unfreeze();
                break;
            case Key.G:
                this.imaging.ImageViewArea.TwoDAcousticViewports[0].PostProcessing.ChangeGrayMapIndex(1);
                break;
            case Key.H:
                this.imaging.ImageViewArea.TwoDAcousticViewports[0].PostProcessing.ChangeGrayMapIndex(-1);
                break;
        }
    }

    internal class MouseCallbackAdapter : PragmaticScene.SceneControl.IMouseCallback
    {
        public event EventHandler<PragmaticScene.SceneControl.MouseEventArgs> OnMouseMove;
        public event EventHandler<PragmaticScene.SceneControl.MouseEventArgs> OnMouseDown;
        public event EventHandler<PragmaticScene.SceneControl.MouseEventArgs> OnMouseUp;


        private Window window;
        private Grid imageAreaGrid;

        public MouseCallbackAdapter(Grid grid, Window window)
        {
            // Use generic MouseDown/MouseUp so all buttons are handled
            grid.MouseDown += ImagingAreaHost_MouseDown;
            grid.MouseUp += ImagingAreaHost_MouseUp;
            grid.MouseMove += ImagingAreaHost_MouseMove;

            this.window = window;
            this.imageAreaGrid = grid;
        }

        private void ImagingAreaHost_MouseMove(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            PragmaticScene.SceneControl.MouseButtons mouseButtons = PragmaticScene.SceneControl.MouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouseButtons |= PragmaticScene.SceneControl.MouseButtons.Left;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                mouseButtons |= PragmaticScene.SceneControl.MouseButtons.Right;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                mouseButtons |= PragmaticScene.SceneControl.MouseButtons.Middle;
            }


            var position = e.GetPosition(this.imageAreaGrid);
            PragmaticScene.SceneControl.MouseEventArgs mouseEventArgs = new PragmaticScene.SceneControl.MouseEventArgs(mouseButtons, 1, (int)position.X, (int)position.Y, 0); // TODO: mouse button not none always
            this.OnMouseMove?.Invoke(this, mouseEventArgs);
        }

        private void ImagingAreaHost_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this.imageAreaGrid);

            var mouseButtons = PragmaticScene.SceneControl.MouseButtons.None;

            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Left;
                    break;
                case System.Windows.Input.MouseButton.Right:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Right;
                    break;
                case System.Windows.Input.MouseButton.Middle:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Middle;
                    break;
                default:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.None;
                    break;
            }

            PragmaticScene.SceneControl.MouseEventArgs mouseEventArgs = new PragmaticScene.SceneControl.MouseEventArgs(mouseButtons, 1, (int)position.X, (int)position.Y, 0);
            this.OnMouseDown?.Invoke(this, mouseEventArgs);
        }

        private void ImagingAreaHost_MouseUp(object? sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this.imageAreaGrid);

            var mouseButtons = PragmaticScene.SceneControl.MouseButtons.None;

            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Left;
                    break;
                case System.Windows.Input.MouseButton.Right:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Right;
                    break;
                case System.Windows.Input.MouseButton.Middle:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.Middle;
                    break;
                default:
                    mouseButtons = PragmaticScene.SceneControl.MouseButtons.None;
                    break;
            }

            PragmaticScene.SceneControl.MouseEventArgs mouseEventArgs = new PragmaticScene.SceneControl.MouseEventArgs(mouseButtons, 1, (int)position.X, (int)position.Y, 0);
            this.OnMouseUp?.Invoke(this, mouseEventArgs);
        }


    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Domain.Service;
using DupploPulse.UsImaging.Infrastracture.Beamformer;
using DupploPulse.UsImaging.Infrastracture.CompositionRoot;
using DupploPulse.UsImaging.Infrastracture.ImageSource;
using DupploPulse.UsImaging.Infrastructure.Common;
using DupploPulse.UsImaging.Infrastructure.Renderer;
using DupploPulse.UsImaging.Infrastructure.Renderer.Renderer;
using DupploPulse.UsImaging.Infrastructure.Renderer.Scene;
using DupploPulse.UsImaging.Infrastructure.Stream;
using PragmaticScene.RenderableInterfaces;
using PragmaticScene.Renderer.Scene;

namespace DupploPulse.UsImaging.Infrastructure.CompositionRoot
{
    public class Composer
    {
        public Composer()
        {
            // setup traces
            TraceSource traceSource = new TraceSource(RendererTraceSource.Performance);
            traceSource.Listeners.Add(new ConsoleTraceListener());
            traceSource = new TraceSource(RendererTraceSource.Common);
            traceSource.Listeners.Add(new ConsoleTraceListener());
            var generalLogger = new ConsoleGeneralLogger();

            var renderedWindow = new PragmaticScene.Scene.Window(1024, 720);
            var renderer = new PragmaticScene.Renderer.Renderer(renderedWindow, PragmaticScene.RendererInterfaces.RenderingEngine.DirectX11, new() { new UsRenderersProviderCreator() });
            renderer.AddSceneObjectRenderersCreator(new SceneObjectRenderersCreator());
            renderer.IsBackgroundRendering = true;

            
            string[] mockDataFiles = new string[10];
            for (int j = 66; j < 76; j++)
            {
                mockDataFiles[j-66] = "Assets//SavedFrame_92_18" + j + ".bin";
            }
            var mockDataLiveSource = new LiveMockFrameSource(mockDataFiles);
            mockDataLiveSource.Start();

            var rendererAdapter = new RendererAdapter(renderer);

            var streamingServer = new GrpcImageServer();
            var interactionServer = new GrpcInteractionServer();

            var cine = new Cine(100);
            var systemTime = new SystemTime();
            var imagingService = new ImagingService(mockDataLiveSource, cine, systemTime, new RgbImageCollector(), streamingServer);
            var creator = new Creator();
            creator.AddCreator(new UsSceneObjectsCreator());
            var postProcessingService = new PostProcessingService(systemTime, creator, renderedWindow, new MouseCallbackAdapter(interactionServer), rendererAdapter);

            var apiCallDeserializer = new DomainApiProxyDeserializer(imagingService as IImaging, generalLogger);

            interactionServer.MouseMove += (o, e) => postProcessingService.ImageViewArea.MouseMove(e.X,e.Y);
            interactionServer.MouseDown += (o, e) => postProcessingService.ImageViewArea.MouseDown(e.X, e.Y);
            interactionServer.MouseUp += (o, e) => postProcessingService.ImageViewArea.MouseUp(e.X, e.Y);
            interactionServer.ApiCall += (o, e) => apiCallDeserializer.Deserialize(e);

            imagingService.SetPostProcessing(postProcessingService);
            
            var beamformingService = new BeamformingService(new BeamformerAdapter());
            imagingService.SetBeamforming(beamformingService);
            
            Console.ReadKey();

        }

       // public IRenderingService RenderingService { get; private set; }

        //private void LoadMockData(Cine cine)
        //{
        //    for (int j = 66; j < 76; j++)
        //    {
        //        using var mmf = MemoryMappedFile.CreateFromFile("Assets//SavedFrame_92_18" + j + ".bin", FileMode.Open);
        //        using var accessor = mmf.CreateViewAccessor();

        //        long fileSize = new FileInfo("Assets//SavedFrame_92_18" + j + ".bin").Length;
        //        byte[] byteArray = new byte[fileSize];
        //        accessor.ReadArray(0, byteArray, 0, (int)fileSize);

        //        var frameReference = new ManagedMemoryFrameReference(byteArray);
        //        cine.PushFrame(frameReference);
        //    }
        //}
    }
}
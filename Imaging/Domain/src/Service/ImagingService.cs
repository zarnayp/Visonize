using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Visonize.UsImaging.Domain.Infrastracture.Common;
using Visonize.UsImaging.Domain.Infrastracture.Player;
using Visonize.UsImaging.Domain.Interfaces;
using Visonize.UsImaging.Domain.Player;
using Visonize.UsImaging.Domain.Service.Framework;
using Visonize.UsImaging.Domain.Service.Infrastructure;
using Visonize.UsImaging.Domain.ValueObjects;

namespace Visonize.UsImaging.Domain.Service
{
    public class ImagingService : IImaging
    {
        private ILiveDataSource liveSource;
        private IBeamformingService? beamforming;
        private IPostProcessingService? postProcessing;
        private IImageStream? imageStream;
        private IRgbFrameCollector frameCollector;
        private ICine cine;
        private ISystemTime systemTime;
        private ICinePlayer cinePlayer;
        private bool isLive = true;
        private Dispatcher dispatcher = new Dispatcher();
        private System.Timers.Timer timer;
        private AutoResetEvent tickEvent = new(true);


        public Dispatcher Dispatcher { get { return dispatcher; } }

        public IImageViewArea ImageViewArea { get; private set; }

        public ImagingService(ILiveDataSource liveSource, ICine cine, ISystemTime systemTime, IRgbFrameCollector frameCollector, IImageStream? imageStream = null)
        {
            this.liveSource = liveSource;
            this.cine = cine;
            this.systemTime = systemTime;
            this.frameCollector = frameCollector;
            this.imageStream = imageStream;
            liveSource.NewRFFrame += LiveSource_NewRFFrame;

            timer = new System.Timers.Timer(1000.0 / 60f); // ~16.666 ms
            timer.Elapsed += OnTick;
            timer.AutoReset = true;
            timer.Start();


            Task newTask = Task.Run(() => ImagingLoop());
        }

        public void SetBeamforming(IBeamformingService beamforming)
        {
            this.beamforming = beamforming;
        }
        
        public void SetPostProcessing(IPostProcessingService postProcessing)
        {
            this.postProcessing = postProcessing;
            //this.PostProcessing = DispatcherProxyCreator.CreateDispatcherProxy(postProcessing, this.dispatcher);
            this.ImageViewArea = DispatcherProxyCreator.CreateDispatcherProxy((IImageViewArea)this.postProcessing.ImageViewArea, this.dispatcher);
        }

        private void LiveSource_NewRFFrame(object? sender, RFFrameDescription e)
        {
            // TODO: create preprocessing thread
            this.dispatcher.BeginInvoke(() =>
            {
                if (beamforming != null)
                {
                    // preProcessing
                    var rawTargetImage = cine.GetTargetFrameBuffer();
                    var beamformedFrame = beamforming.ProcessRFData(e, rawTargetImage);
                    cine.PushedTargetFrame(beamformedFrame.RawFrameMetaData);

                    // postProcessing
                    var rgbTargetImage = this.frameCollector.GetTargetImage();

                  this.postProcessing?.ImageViewArea.UpdateImage(beamformedFrame.RawFrameReference, new SceneObjects.CurvedGeometryData());

                    //this.postProcessing?.ProcessRawData(beamformedFrame, rgbTargetImage);
                    //this.imageStream?.UpdateImage(rgbTargetImage);
                }
            });
        }


        // Main postProcessing loop in 60Hz.
        private void ImagingLoop()
        {
            while (true)
            {
                tickEvent.WaitOne();
                this.dispatcher.Dispatch();

                //if(!this.isLive)  // Cine pipeline 
                //{
                    var rgbTargetImage = this.frameCollector.GetTargetImage();
                    this.postProcessing?.Process(rgbTargetImage);
                    this.imageStream?.UpdateImage(rgbTargetImage);
                //}

               
            }
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            tickEvent.Set();
        }


        public void Freeze()
        {
            this.dispatcher.BeginInvoke(() =>
            {
                this.InternalFreeze();
                this.postProcessing?.ImageViewArea.SwitchToCine(this.cine);
            });
            this.isLive = false;
        }

        public void Unfreeze()
        {
            this.isLive = true;
            this.dispatcher.BeginInvoke(() =>
            {
                this.InternalUnfreeze();
                this.postProcessing?.ImageViewArea.SwitchToLive();
            });
        }

        private void InternalFreeze()
        {
            this.liveSource.NewRFFrame -= LiveSource_NewRFFrame;
            this.isLive = false;
        }

        private void InternalUnfreeze()
        {
            this.isLive = true;
            this.liveSource.NewRFFrame += LiveSource_NewRFFrame;
        }
    }
}
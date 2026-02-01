using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Player;
using DupploPulse.UsImaging.Domain.ValueObjects;
using DupploPulse.UsImaging.Infrastructure.Common;
using PragmaticScene.Scene;

namespace DupploPulse.UsImaging.Infrastracture.ImageSource
{
    public class LiveMockFrameSource : ILiveDataSource
    {
        private bool stopSignal = false;
        private ManagedMemoryFrameReference[] data;
        public event EventHandler<RFFrameDescription>? NewRFFrame;

        public LiveMockFrameSource(string[] binFiles)
        {
            data = new ManagedMemoryFrameReference[binFiles.Length];
            int i = 0;
            foreach (var binFile in binFiles)
            {
                using var mmf = MemoryMappedFile.CreateFromFile(binFile, FileMode.Open);
                using var accessor = mmf.CreateViewAccessor();
                long fileSize = new FileInfo(binFile).Length;

                byte[] byteArray = new byte[fileSize];
                accessor.ReadArray(0, byteArray, 0, (int)fileSize);

                var frameReference = new ManagedMemoryFrameReference(byteArray);
                data[i] = frameReference;
                i++;
            }
        }

        public void Start()
        {
            Task.Run(() =>
            {
                int i = 0;
                while (!stopSignal)
                {
                    if (i == this.data.Length)
                    {
                        i = 0;
                    }

                    RFFrameDescription rawFrameDescription = new RFFrameDescription(data[i]);

                    NewRFFrame?.Invoke(this, rawFrameDescription);

                    Thread.Sleep(32);

                    i++;
                }
            });
        }

        public void Stop()
        {
            this.stopSignal = true;
        }
    }
}

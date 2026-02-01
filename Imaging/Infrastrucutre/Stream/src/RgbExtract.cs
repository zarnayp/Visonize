using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Infrastructure.Stream;

// disabled to achieve best performance
#pragma warning disable CS8618 

public class RgbExtract : IDisposable
{ 
    //private int numberOfThreads;
    private bool exit = false;
    
    List<AutoResetEvent> eventList = new ();
    List<AutoResetEvent> startEventList = new ();
        
    public RgbExtract(int componentSize, int numOfThreads)
    {
        if (numOfThreads <= 0)
        {
            throw new ArgumentException("Number of threads must be > 0");
        }

        int threadShift = 0;
        for (int it = 0; it < numOfThreads; it++)
        {
            var startEvent = new AutoResetEvent(false);
            var doneEvent = new AutoResetEvent(false);
            eventList.Add(doneEvent); 
            startEventList.Add(startEvent);
            
            int threadLoad = (componentSize) / numOfThreads;
            if (it == 0)
            {
                threadLoad = threadLoad + componentSize  % numOfThreads;;
            }

            var threadIndex = it;
            var currentThreadShift = threadShift;
            
            threadShift += threadLoad;
            
            ThreadStart threadStart = () => WorkerThread(threadIndex, startEvent, doneEvent, currentThreadShift, threadLoad);
            
            new Thread(threadStart).Start();
            

        }
    }

    private byte[] red;
    private byte[] green;
    private byte[] blue;
    private byte[] rgba;
    
    private void WorkerThread(int threadIndex, AutoResetEvent startEvent, AutoResetEvent doneEvent, int threadShift, int threadLoad)
    {
        while (true)
        {
            startEvent.WaitOne();

            if (exit)
            {
                break;
            }
            for (int i = 0; i < threadLoad; i++)
            {
                red[i + threadShift] = rgba[i * 4 + threadShift * 4 ];
                green[i + threadShift] = rgba[i * 4 + 1 + threadShift * 4 ];
                blue[i + threadShift] = rgba[i * 4 + 2 + threadShift * 4 ];
            }

            doneEvent.Set();
        }
        doneEvent.Set();
    }
    
    public void ExtractRGBA(byte[] outRed, byte[] outGreen, byte[] outBlue, byte[] inputRgba)
    {
        this.blue = outBlue;
        this.green = outGreen;
        this.red = outRed;
        this.rgba = inputRgba;
        
        foreach (var startEvent in startEventList)
        {
            startEvent.Set();           
        }

        WaitHandle.WaitAll(eventList.ToArray());
    }

    public void Dispose()
    {
        this.exit = true;
        
        foreach (var startEvent in startEventList)
        {
            startEvent.Set();           
        }
        WaitHandle.WaitAll(eventList.ToArray());
    }
    
#pragma warning restore CS8618
    
}
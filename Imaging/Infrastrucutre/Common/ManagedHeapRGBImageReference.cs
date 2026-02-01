using DupploPulse.UsImaging.Domain.Service.Infrastructure;

namespace DupploPulse.UsImaging.Infrastructure.Common;

public class ManagedHeapRGBImageReference : IRgbImageReference
{
    public ManagedHeapRGBImageReference(byte[] rgb)
    {
        this.Rgb = rgb;
    }
    
    public byte[] Rgb { get; private set; }
    
}
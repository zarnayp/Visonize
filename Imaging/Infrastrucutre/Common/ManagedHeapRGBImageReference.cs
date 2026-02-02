using Visonize.UsImaging.Domain.Service.Infrastructure;

namespace Visonize.UsImaging.Infrastructure.Common;

public class ManagedHeapRGBImageReference : IRgbImageReference
{
    public ManagedHeapRGBImageReference(byte[] rgb)
    {
        this.Rgb = rgb;
    }
    
    public byte[] Rgb { get; private set; }
    
}
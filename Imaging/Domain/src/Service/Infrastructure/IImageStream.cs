namespace DupploPulse.UsImaging.Domain.Service.Infrastructure;

public interface IImageStream
{
    void UpdateImage(IRgbImageReference image);
}
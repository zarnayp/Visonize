using DupploPulse.UsImaging.Domain.Service.Infrastructure;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastructure.CompositionRoot;

public class RendererAdapter : IRenderer
{
    private readonly PragmaticScene.SceneInterfaces.ISceneRenderer renderer;
    
    public RendererAdapter(PragmaticScene.SceneInterfaces.ISceneRenderer renderer)
    {
        this.renderer = renderer;
    }

    public PragmaticScene.SceneInterfaces.ISceneRenderer PragmaticSceneRenderer => this.renderer;

    public void Render()
    {
        this.renderer.Render();
    }

    public void RenderWithReadback(IRgbImageReference imageReference)
    {
        var buffer = ((ManagedHeapRGBImageReference)imageReference).Rgb;
        this.renderer.RenderWithReadback(buffer);
    }

    //public IRgbImageReference GetLatestData()
    //{
    //    return new ManagedHeapRGBImageReference(this.renderer.GetLatestData());
    //}
}
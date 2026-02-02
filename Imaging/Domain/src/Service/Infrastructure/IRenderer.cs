namespace Visonize.UsImaging.Domain.Service.Infrastructure;

public interface IRenderer
{
    void Render();

    void RenderWithReadback(IRgbImageReference imageReference);

    PragmaticScene.SceneInterfaces.ISceneRenderer PragmaticSceneRenderer { get; }
}
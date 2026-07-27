using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Glue;

namespace AvoidClaws.code.dotnet.Controllers;

public interface IController : IGameObject
{
    public void Attach(IActor actor);
    public void Detach(IActor actor);

    public IActor? GetAttachment();
}
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Controllers;

public interface IController : IKableObject
{
    public void Attach(IActor actor);
    public void Detach(IActor actor);

    public IActor? GetAttachment();

    void HandleNetTick(uint currentTick);
}
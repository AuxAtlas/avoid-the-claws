using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Buffs;

public interface IBuff : IKableObject
{
    public IActor OwnerActor { get; }

    public void SetupBuff(IActor actor);
    public void DestroyBuff();
    public void Tick();
}
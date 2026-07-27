using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Glue;

namespace AvoidClaws.code.dotnet.Buffs;

public interface IBuff : IGameObject
{
    public IActor OwnerActor { get; }

    public void SetupBuff(IActor actor);
}
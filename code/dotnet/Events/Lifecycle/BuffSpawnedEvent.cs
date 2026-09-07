using AvoidClaws.code.dotnet.Buffs;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public record BuffSpawnedEvent : GameEvent
{
    public IBuff? Buff { get; set; }
}
#region

using AvoidClaws.code.dotnet.Actors;

#endregion

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public record ActorSpawnedEvent : GameEvent
{
    public IActor? Actor { get; set; }
}
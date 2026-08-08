#region

using AvoidClaws.code.dotnet.Actors;

#endregion

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ActorSpawnedEvent : GameEvent
{
    public IActor? Actor { get; set; }
}
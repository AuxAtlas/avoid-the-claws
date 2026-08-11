#region

using AvoidClaws.code.dotnet.Glue;

#endregion

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class GameObjectDespawnedEvent : GameEvent
{
    public required IGameObject GameObject { get; init; }
}
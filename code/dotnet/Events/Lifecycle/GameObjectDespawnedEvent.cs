using AvoidClaws.code.dotnet.Glue;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class GameObjectDespawnedEvent : GameEvent
{
    public IGameObject GameObject { get; set; }
}
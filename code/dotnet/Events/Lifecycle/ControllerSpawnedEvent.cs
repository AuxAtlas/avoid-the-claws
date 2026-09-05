#region

using AvoidClaws.code.dotnet.Controllers;

#endregion

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public record ControllerSpawnedEvent : GameEvent
{
    public required IController? Controller { get; init; }
}
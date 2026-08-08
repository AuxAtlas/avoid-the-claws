#region

using AvoidClaws.code.dotnet.Controllers;

#endregion

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ControllerSpawnedEvent : GameEvent
{
    public IController? Controller { get; set; }
}
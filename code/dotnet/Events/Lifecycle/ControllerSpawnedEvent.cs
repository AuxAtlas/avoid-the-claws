using AvoidClaws.code.dotnet.Controllers;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ControllerSpawnedEvent : GameEvent
{
    public IController? Controller { get; set; }
}
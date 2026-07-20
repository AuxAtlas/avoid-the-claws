using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Levels;
using Godot;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ControllerSpawnedEvent : GameEvent
{
    public IController? Controller { get; set; }
    public Node? RootNode { get; set; }
    public GameWorld? Level { get; set; }
}
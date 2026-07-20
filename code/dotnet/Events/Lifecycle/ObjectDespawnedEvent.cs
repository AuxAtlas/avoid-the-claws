using AvoidClaws.code.dotnet.Levels;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ObjectDespawnedEvent : GameEvent
{
    public IKableObject KableObject { get; set; }
    public Node RootNode { get; set; }
    public GameWorld GameWorld { get; set; }
}
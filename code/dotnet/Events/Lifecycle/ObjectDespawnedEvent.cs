using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ObjectDespawnedEvent : GameEvent
{
    public IKableObject KableObject { get; set; }
    public Node RootNode { get; set; }
    public GameWorld GameWorld { get; set; }
}
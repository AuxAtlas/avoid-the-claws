using AvoidClaws.code.dotnet.Actors;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

namespace AvoidClaws.code.dotnet.Events.Lifecycle;

public class ActorSpawnedEvent : GameEvent
{
    public IActor? Actor { get; set; }
}
using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class ActorPrefabs : Node
{
    [Export]
    public PackedScene? PlayerActorPrefab { get; private set; }
}
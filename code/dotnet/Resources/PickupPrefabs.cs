using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class PickupPrefabs : Node
{
    [Export]
    public PackedScene? HealthPickupPrefab { get; private set; }

    [Export]
    public PackedScene? SpeedPickupPrefab { get; private set; }
}
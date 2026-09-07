using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class BuffPrefabs : Node
{
    [Export]
    public PackedScene? SpeedBuffPrefab { get; private set; }
}
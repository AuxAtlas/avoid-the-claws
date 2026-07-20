using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class ControllerPrefabs : Node
{
    [Export]
    public PackedScene? DummyControllerPrefab { get; private set; }

    [Export]
    public PackedScene? PlayerControllerPrefab { get; private set; }
}
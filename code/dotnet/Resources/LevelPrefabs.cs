#region

using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Resources;

public partial class LevelPrefabs : Node
{
    [Export]
    public PackedScene DevEnvMap { get; private set; } = null!;
}
#region

using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Resources;

public partial class ScreenPrefabs : Node
{
    [Export]
    public PackedScene MainMenuScreen { get; private set; } = null!;

    [Export]
    public PackedScene JoinGameScreen { get; private set; } = null!;

    [Export]
    public PackedScene OptionsScreen { get; private set; } = null!;

    [Export]
    public PackedScene LoadingScreen { get; private set; } = null!;
}
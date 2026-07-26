using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class ScreenPrefabs : Node
{
    [Export]
    public PackedScene? MainMenuScreen { get; private set; }

    [Export]
    public PackedScene? JoinGameScreen { get; private set; }

    [Export]
    public PackedScene? OptionsScreen { get; private set; }

    [Export]
    public PackedScene? LoadingScreen { get; private set; }
}
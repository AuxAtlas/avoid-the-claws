using Godot;

namespace AvoidClaws.code.dotnet.Screens;

public partial class GameScreenHandles : Node
{
    [Export]
    public Control MainMenuScreenHandle { get; private set; } = null!;

    [Export]
    public Control OptionsScreenHandle { get; private set; } = null!;

    [Export]
    public Control JoinGameScreenHandle { get; private set; } = null!;

    public void HideAll()
    {
        MainMenuScreenHandle.Hide();
        OptionsScreenHandle.Hide();
        JoinGameScreenHandle.Hide();
    }
}
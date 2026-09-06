using AvoidClaws.code.dotnet.Data.State;
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

    public override void _Input(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);
        if (Input.IsActionJustPressedByEvent(ActionName.Pause.ToActionString(), @event))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
    }
}
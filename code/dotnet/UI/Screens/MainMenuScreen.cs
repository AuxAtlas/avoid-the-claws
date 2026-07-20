using AvoidClaws.code.dotnet.Data;
using Godot;

namespace AvoidClaws.code.dotnet.UI.Screens;

public partial class MainMenuScreen : BasicScreen
{
    [Export] private Label? _errorMessageLabel;

    [Export] private Control? _errorMessagePanel;

    public override void _Ready()
    {
        base._Ready();

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ErrorMessage? messageToShow = null;

        if (Core.World.DisplayedErrorMessages.Count > 0)
        {
            messageToShow = Core.World.DisplayedErrorMessages[0];
            foreach (var errorMessage in Core.World.DisplayedErrorMessages)
                if (errorMessage.SecondsRemaining < messageToShow.SecondsRemaining)
                    messageToShow = errorMessage;
        }

        SetErrorMessage(messageToShow);
    }

    private void SetErrorMessage(ErrorMessage? msg)
    {
        SetErrorMessage(msg?.Message, msg?.SecondsRemaining ?? 0f);
    }

    private void SetErrorMessage(string? errorMessage, float messageTimeout = 10.0f)
    {
        if (string.IsNullOrEmpty(errorMessage) || messageTimeout <= 0)
        {
            _errorMessageLabel?.Text = string.Empty;
            _errorMessagePanel?.SetVisible(false);
        }
        else
        {
            _errorMessageLabel?.Text = errorMessage;
            _errorMessagePanel?.SetVisible(true);
        }
    }

    private void HandleJoinButtonClicked()
    {
        Core.World.ChangeMapTo(Core.Resources.ScreenPrefabs.JoinGameScreen);
    }

    private void HandleHostButtonClicked()
    {
        Core.Network.HostServer();
    }

    private void HandleQuitButtonClicked()
    {
        GD.Print("Quitting game by request...");
        GetTree().Quit();
    }
}
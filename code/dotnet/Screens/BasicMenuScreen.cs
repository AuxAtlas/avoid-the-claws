#region

using AvoidClaws.code.dotnet.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Screens;

public abstract partial class BasicMenuScreen : Control
{
    [Inject]
    public CoreGame Core { get; private set; } = null!;
    
    [Export]
    private Label? _errorMessageLabel;

    [Export]
    private Control? _errorMessagePanel;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    

    public override void _Process(double delta)
    {
        base._Process(delta);

        ErrorMessage? messageToShow = null;

        if (Core.DisplayedErrorMessages.Count > 0)
        {
            messageToShow = Core.DisplayedErrorMessages[0];
            foreach (var errorMessage in Core.DisplayedErrorMessages)
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
}
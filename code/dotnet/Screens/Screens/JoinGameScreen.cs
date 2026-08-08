#region

using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Screens.Screens;

public partial class JoinGameScreen : BasicScreen
{
    [Export]
    private LineEdit? _hostAddressInput;

    public override void _Ready()
    {
        if (_hostAddressInput == null)
            GD.PrintErr("JoinGameMenuController: 'HostAddressInput' node is not set");
    }

    private void HandleJoinButtonClicked()
    {
        if (_hostAddressInput == null)
            return;

        Core.World.ChangeMapTo(Core.Resources.ScreenPrefabs.LoadingScreen);

        Core.Network.ConnectToHost(_hostAddressInput!.Text);
    }

    private void HandleBackButtonClicked()
    {
        Core.World.ChangeMapTo(Core.Resources.ScreenPrefabs.MainMenuScreen);
    }
}
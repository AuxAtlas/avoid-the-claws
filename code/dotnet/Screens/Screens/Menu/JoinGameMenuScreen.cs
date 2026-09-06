#region

using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Screens.Screens.Menu;

public partial class JoinGameMenuScreen : BasicMenuScreen
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
        
        Core.Screens.HideAll();
        Core.Screens.Layers.LoadingLayer.Show();

        Core.Network.ConnectToHost(_hostAddressInput!.Text);
    }

    private void HandleBackButtonClicked()
    {
        Core.Screens.DisplayMainMenuScreen();
    }
}
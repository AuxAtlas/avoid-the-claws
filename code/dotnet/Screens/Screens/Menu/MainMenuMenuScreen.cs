#region

using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Screens.Screens.Menu;

public partial class MainMenuMenuScreen : BasicMenuScreen
{
    private void HandleJoinButtonClicked()
    {
        // TODO: Implement JoinButtonClicked
    }

    private void HandleHostButtonClicked()
    {
        Core.Screens.DisplayLoadingScreen();
        Core.Network.HostServer();
    }

    private void HandleOptionsButtonClicked()
    {
        // TODO: Add options screen
    }

    private void HandleQuitButtonClicked()
    {
        GD.Print("Quitting game by request...");
        GetTree().Quit();
    }
}
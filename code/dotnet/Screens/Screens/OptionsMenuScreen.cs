using AvoidClaws.code.dotnet.Data;
using Godot;

namespace AvoidClaws.code.dotnet.Screens.Screens;

public partial class OptionsMenuScreen : BasicMenuScreen
{
    private void HandleBackButtonClicked()
    {
        Core.Screens.DisplayMainMenuScreen();
    }
}
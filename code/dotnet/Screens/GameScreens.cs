using AvoidClaws.code.dotnet.Resources;
using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.Screens;

public partial class GameScreens : Node, IService
{
    [Export]
    public GameScreenLayers Layers { get; private set; } = null!;

    [Export]
    public GameScreenHandles Handles { get; private set; } = null!;

    public void HideAll()
    {
        Layers.HideAll();
        Handles.HideAll();
    }

    public void DisplayMainMenuScreen()
    {
        HideAll();
        Layers.ScreensLayer.Show();
        Handles.MainMenuScreenHandle.Show();
    }
    public void DisplayLoadingScreen()
    {
        HideAll();
        Layers.LoadingLayer.Show();
    }
}
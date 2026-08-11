using Godot;

namespace AvoidClaws.code.dotnet.Screens;

public partial class GameScreenLayers : Node
{
    [Export]
    public CanvasLayer ScreensLayer { get; private set; } = null!;

    [Export]
    public CanvasLayer PauseLayer { get; private set; } = null!;

    [Export]
    public CanvasLayer LoadingLayer { get; private set; } = null!;
    
    [Export]
    public CanvasLayer HudLayer { get; private set; } = null!;
    
    [Export]
    public CanvasLayer DebugLayer { get; private set; } = null!;

    public void HideAll()
    {
        ScreensLayer.Hide();
        PauseLayer.Hide();
        LoadingLayer.Hide();
        HudLayer.Hide();
    }
}
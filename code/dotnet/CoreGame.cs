using AvoidClaws.code.dotnet.Events;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Resources;
using AvoidClaws.code.dotnet.Services;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

namespace AvoidClaws.code.dotnet;

public partial class CoreGame : Node, IService
{
    public enum ActorType : ushort
    {
        Player,

        HealthPickup,
        SpeedPickup
    }

    public enum ControllerType : ushort
    {
        Dummy = 1,
        LocalPlayer = 2
    }

    public enum BuffType : ushort
    {
        Speed
    }

    [Inject]
    public EventBus EventBus { get; } = null!;

    [Inject]
    public NetworkManager Network { get; } = null!;

    [Inject]
    public GameResources Resources { get; } = null!;

    [Inject]
    public GameWorld World { get; } = null!;

    public void CriticalError(string message)
    {
        GD.PushError($"[Critical Error] {message}");
        World.SetDisplayedErrorMessage($"[Critical Error] {message}");
        World.GotoMainMenu();
    }
}
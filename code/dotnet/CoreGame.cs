using System;
using System.Collections.Generic;
using AvoidClaws.code.dotnet.Data;
using AvoidClaws.code.dotnet.Events;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Resources;
using AvoidClaws.code.dotnet.Services;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

namespace AvoidClaws.code.dotnet;

public partial class CoreGame : Node, IService
{
    #region ENUMS
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

    #endregion

    #region INJECTIONS
    [Inject]
    public EventBus EventBus { get; } = null!;

    [Inject]
    public NetworkManager Network { get; } = null!;

    [Inject]
    public GameResources Resources { get; } = null!;

    [Inject]
    public GameWorld World { get; } = null!;

    #endregion

    public Random Random { get; private set; } = null!;

    public bool IsClient => !IsServer;
    public bool IsServer => Network.IsServer;

    public readonly List<ErrorMessage> DisplayedErrorMessages = new();

    public override void _EnterTree()
    {
        Random = new Random(Guid.NewGuid().GetHashCode());
    }

    public override void _Ready()
    {
        World.ChangeMapTo(Resources.ScreenPrefabs.MainMenuScreen);
    }

    public void CriticalError(string message)
    {
        GD.PushError($"[Critical Error] {message}");
        World.SetDisplayedErrorMessage($"[Critical Error] {message}");
        World.GotoMainMenu();
    }

    public KableId GenerateKableId()
    {
        return new KableId(GenerateRawKableId());
    }

    public uint GenerateRawKableId()
    {
        uint result = 0;
        while (result == 0)
            result = (uint)Random.Next() + (uint)Random.Next();
        return result;
    }
}
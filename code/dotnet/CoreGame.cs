#region

using System;
using System.Collections.Generic;
using AvoidClaws.code.dotnet.Data;
using AvoidClaws.code.dotnet.Events;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Resources;
using AvoidClaws.code.dotnet.Screens;
using AvoidClaws.code.dotnet.Services;
using Godot;
using GameWorld = AvoidClaws.code.dotnet.World.GameWorld;

#endregion

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
	public EventBus EventBus { get; private set; } = null!;

	[Inject]
	public NetworkManager Network { get; private set; } = null!;

	[Inject]
	public GameResources Resources { get; private set; } = null!;

	[Inject]
	public GameWorld World { get; private set; } = null!;

	[Inject]
	public GameScreens Screens { get; private set; } = null!;

	[Export]
	private DependencyManager _dependencyManager;

	#endregion

	public Random Random { get; private set; } = null!;

	public bool IsClient => !IsServer;
	public bool IsServer => Network.IsServer;

	public readonly List<ErrorMessage> DisplayedErrorMessages = new();
	
	private Viewport _rootViewport;
	internal Rid PhysicsSpace3DRid => _rootViewport.World3D.Space;

	public override void _EnterTree()
	{
		Random = new Random(Guid.NewGuid().GetHashCode());
	}

	public override void _Ready()
	{
		// _dependencyManager.ReconstructDependencies();
		// _dependencyManager.ReinjectDependencies();
		
		_rootViewport = GetViewport();
		PhysicsServer3D.SpaceSetActive(PhysicsSpace3DRid, false);
		
		Screens.DisplayMainMenuScreen();
	}

	public void CriticalError(string message)
	{
		GD.PushError($"[Critical Error] {message}");
		DisplayedErrorMessages.Add
		(
			new ErrorMessage
			{
				Message = $"[Critical Error] {message}",
				SecondsRemaining = 10f
			}
		);
		World.ResetWorld();
		Screens.DisplayMainMenuScreen();
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

	internal void ProcessNetTick(uint tick)
	{
		for (var i = 0; i < DisplayedErrorMessages.Count; i++)
		{
			DisplayedErrorMessages[i].SecondsRemaining -= NetworkManager.TickDeltaTimeF;
			if (DisplayedErrorMessages[i].SecondsRemaining <= 0) DisplayedErrorMessages.RemoveAt(i);
		}

		World.ProcessNetTick(tick);
	}
}

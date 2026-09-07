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
using AvoidClaws.code.dotnet.World;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet;

public partial class CoreGame : Node, IService
{
#region ENUMS

	public enum ActorTypesEnum : ushort
	{
		Player,
	}

	public enum ControllerTypesEnum : ushort
	{
		Dummy = 1,
		LocalPlayer = 2
	}

	public enum BuffTypesEnum : ushort
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

#endregion

	public Random Random { get; private set; } = new(Guid.NewGuid().GetHashCode());

	public bool IsClient => !IsServer;
	public bool IsServer => Network.IsServer;

	public readonly List<ErrorMessage> DisplayedErrorMessages = new();

    private Rid _rootWorld3DSpaceRid;
    
	public override void _Ready()
    {
        _rootWorld3DSpaceRid = GetTree().Root.World3D.Space;
		PhysicsServer3D.SpaceSetActive(_rootWorld3DSpaceRid, false);
		
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

	public KableId GenerateUniqueKableId()
    {
        uint tmp = GenerateRawKableId();
        while (World.DoesKableIdExist(tmp))
        {
            tmp = GenerateRawKableId();
        }

        return new KableId(tmp);
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
			if (DisplayedErrorMessages[i].SecondsRemaining <= 0)
                DisplayedErrorMessages.RemoveAt(i);
		}
        
		World.ProcessNetTick(tick);
	}
    
    /// <summary>
    /// Steps forward one physics frame using RapierPhysics
    /// </summary>
    public void StepPhysics3D(double delta)
    {
        PhysicsServer3D.Singleton.Call("space_step", _rootWorld3DSpaceRid, delta);
    }
    
    /// <summary>
    /// Flush all current RapierPhysicsServer data into Godot physics nodes
    /// </summary>
    public void FlushPhysics3D()
    {
        PhysicsServer3D.Singleton.Call("space_flush_queries", _rootWorld3DSpaceRid);
    }

    public void LogDebug(string message)
    {
        if(OS.HasFeature("debug"))
            GD.PushError($"[Core.LogDebug] {message}");
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Components;
using AvoidClaws.code.dotnet.Components.Core;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.Actors;

public abstract partial class LivingActor : CharacterBody3D, IActor
{
    [Signal]
    public delegate void RespawnTimerChangedEventHandler(float respawnTimerRemaining);

    [Inject]
    public CoreGame Core { get; } = null!;

#region EXPORTS

    [Export]
    public BoxShape3D? HurtBox { get; private set; }

    [Export]
    public Camera3D? Camera { get; protected set; }
    [Export]
    public Node3D? CameraAnchor { get; protected set; }

    [Export]
    protected Node3D? ComponentContainer { get; private set; }
    [Export]
    protected Node3D? BuffsContainer { get; private set; }

    [Export]
    private int _maxMoveBounces = 4;

    [Export]
    private float _respawnTimeSeconds = 5f;

    private float _respawnTimer;

#endregion
#region VARIABLES

    private bool _isClientFocused;

    private Vector2 _moveInput = Vector2.Zero;
    private Vector2 _rotateInput = Vector2.Zero;
    private byte _attackInputsPacked;
    private byte _actionInputsPacked;

    private uint _lastInputReceivedTick;
    private uint _latestStateTickReceived;

    private ushort _ticksSinceStateSent;

    public bool IsDead => HealthComponent?.IsDead ?? false;
    public uint CurrentTick => Core.Network.NetworkTick;
    public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
    public bool Destroyed => IsQueuedForDeletion();

    protected ObjectState[] StateHistory = new ObjectState[NetworkManager.MAX_TICK_SEQUENCE];
    protected ActorInput Inputs = new ActorInput();

    protected readonly List<IComponent> Components = new();
    protected readonly List<IBuff> Buffs = new();

    public KableId KableId { get; private set; }

    public KableConnectionId AuthorityConnectionId { get; protected set; }
    public uint SpawnedOnTick { get; private set; }
    public bool ReconciliationMode { get; private set; }

    protected HealthComponent? HealthComponent;

    private Vector3 _previousPosition = Vector3.Zero;
    private Vector3 _realVelocity = Vector3.Zero;
    private Vector3 _lastMotion = Vector3.Zero;

    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;

#endregion

public void KableSetup(KableId connectionId)
	{
		KableId = connectionId;
	}

	public override void _Ready()
	{
		base._Ready();

		if (BuffsContainer == null)
			GD.PrintErr("LivingActor: Buffs Container not set.");

		if (ComponentContainer == null)
			GD.PrintErr("LivingActor: ComponentContainer not set.");

		SpawnedOnTick = CurrentTick;

		if (ComponentContainer != null)
		{
			Components.Clear();
			foreach (Node child in ComponentContainer.GetChildren())
			{
				if (child is IComponent component)
					Components.Add(component);
			}

			CacheComponentReferences();

			Components.ForEach(x => x.SetupComponent());
		}
	}

	protected virtual void CacheComponentReferences()
	{
		if (HealthComponent is not null)
		{
			HealthComponent.Died -= HandleDiedEvent;
			HealthComponent.HealthChanged -= HandleHealthChanged;
		}

		foreach (IComponent component in Components)
		{
			switch (component)
			{
				case HealthComponent healthComponent:
					HealthComponent = healthComponent;
					break;
			}
		}

		if (HealthComponent is not null)
		{
			HealthComponent.Died += HandleDiedEvent;
			HealthComponent.HealthChanged += HandleHealthChanged;
		}
	}

	public virtual void Respawn()
	{
		_respawnTimer = -1f;

		HealthComponent?.Revive();
		Velocity = Vector3.Zero;

		if (IsClient)
			return;

		Vector3 spawnPos = new(-300f + Core.World.Random.NextSingle() * 500f, 10f, -300f + Core.World.Random.NextSingle() * 500f);
		TeleportTo(spawnPos);
	}




	public T? GetComponent<T>() where T : IComponent
	{
		return (T?)Components.FirstOrDefault(x => x is T);
	}

	public void SetMovementInput(Vector2 input)
	{
		_moveInput = input;
	}

	public void SetRotationInput(Vector2 input)
	{
		_rotateInput = input;
	}

	public void SetAttackInputsPacked(byte input)
	{
		_attackInputsPacked = input;
	}
	public void SetActionInputsPacked(byte input)
	{
		_actionInputsPacked = input;
	}

	public virtual void SetClientFocused()
	{
		Camera?.MakeCurrent();
	}

	public virtual void ProcessInput(uint tickToProcess)
	{
		if (HealthComponent?.IsDead == true)
			return;

		ProcessInputCustom(TickDeltaTimeF, Inputs);

		MoveAndSlide(TickDeltaTimeF);
	}

	public void SetKableAuthority(KableConnectionId connectionId)
	{
		AuthorityConnectionId = connectionId;
	}


	public virtual void HandleNetTick(uint tick)
	{
		if (Destroyed || !Core.Network.FinishedInitialSync)
			return;

		ApplyDebugString();

		List<IBuff> tmpBuffRefs = new(Buffs);
		tmpBuffRefs.ForEach(x => x.Tick());

		ProcessInput(tick);

		if (IsServer)
		{
			var state = GetCurrentState();

			IngestNetworkState(state);
		}

		// TODO: Add Client2Server input sending
		// if (NetworkManager.IsServer || AuthorityConnectionId.Equals(NetworkManager.MyConnectionId))
		// {
		//     SendInputSync(currentTick);
		// }

		if (_respawnTimer > 0d && IsDead)
		{
			_respawnTimer -= TickDeltaTimeF;
			if (_respawnTimer <= 0f)
			{
				Respawn();
			}

			EmitSignalRespawnTimerChanged(_respawnTimer);
		}

		ForceUpdateTransform();
	}

	public void ApplyBuff(IBuff buff)
	{
		throw new NotImplementedException();
	}

	public void RemoveBuff(IBuff buff)
	{
		throw new NotImplementedException();
	}

	public ObjectState GetCurrentState()
	{
		ObjectState state = new ObjectState()
		{
			NetworkTick = CurrentTick,
			ObjectId = KableId,
			AuthorityConnectionId = AuthorityConnectionId
		};

		state.Put(Position);
		state.Put(Rotation);
		state.Put(Velocity);

		state.Put(Inputs.MoveInput);
		state.Put(Inputs.LookInput);
		state.Put(Inputs.AttackInputsPacked);
		state.Put(Inputs.ActionInputsPacked);

		return state;
	}
	public void SetCurrentState(ObjectState state)
	{
		AuthorityConnectionId = state.AuthorityConnectionId;

		Position = state.ReadVector3();
		Rotation = state.ReadVector3();
		Velocity = state.ReadVector3();

		Inputs.MoveInput = state.ReadVector2();
		Inputs.LookInput = state.ReadVector2();
		Inputs.AttackInputsPacked = state.ReadByte();
		Inputs.ActionInputsPacked = state.ReadByte();
	}
	public void IngestNetworkState(ObjectState state)
	{
		if (IsServer)
			throw new InvalidOperationException();

		StateHistory[state.NetworkTick % NetworkManager.MAX_TICK_SEQUENCE] = state;
		SetCurrentState(state);
	}

	public void ResetInputs()
	{
		_moveInput = Vector2.Zero;
		_rotateInput = Vector2.Zero;
		_attackInputsPacked = 0;
		_actionInputsPacked = 0;
	}

	
	public ImmutableArray<IBuff> GetBuffs()
	{
		return Buffs.ToImmutableArray();
	}


	public void TakeDamage(float amount, IKableObject? source)
	{
		HealthComponent?.TakeDamage(amount);
	}

	public void TakeHeal(float amount, IKableObject? source)
	{
		HealthComponent?.TakeHeal(amount);
	}
	public void Destroy(IKableObject? source)
	{
		HealthComponent?.TakeDamage(HealthComponent.CurrentHealth);
	}

	protected virtual void HandleDiedEvent()
	{
		_respawnTimer = _respawnTimeSeconds;

		if (IsServer)
			TeleportTo(new Vector3(0f, -100f, 0f));
	}

	public void Destroy()
	{
		if (Destroyed)
			return;

		QueueFree();
	}

	// TODO: Implement net reconciliation

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (CameraAnchor != null)
			Camera?.GlobalTransform = CameraAnchor.GetGlobalTransformInterpolated();
	}

	public virtual void LookAt(Vector3 target)
	{
		LookAt(target, Vector3.Zero);
	}

	public void TeleportTo(Vector3 position, Vector3? rotation = null)
	{
		GlobalPosition = position;
		if (rotation is not null)
			GlobalRotation = rotation.Value;
	}


	public void TryPlaySound(AudioStream? soundEffect, float pitch = 1.0f, bool interrupt = false)
	{
		if (ReconciliationMode)
			return;

		if (soundEffect is null)
			return;

		GetComponent<AudioPlayerComponent>()?.Play(soundEffect, pitch, interrupt);
	}


	public void MoveAndSlide(float deltaTimeF)
	{
		Vector3 motion = Velocity * deltaTimeF;
		KinematicCollision3D result = MoveAndCollide(motion);

		for (int i = 0; i < _maxMoveBounces; i++)
		{
			if (result == null)
				break;

			motion = result.GetRemainder().Slide(result.GetNormal());
			Velocity = Velocity.Slide(result.GetNormal());
			result = MoveAndCollide(motion);
		}
	}

	public bool IsLogicAuthority()
	{
		return AuthorityConnectionId == Core.Network.MyConnectionId;
	}

	private void ApplyDebugString()
	{
		string debugString = "[DEBUG]\n";

		debugString += $"KableId={KableId.Id}\n";
		debugString += $"AuthorityConnId={AuthorityConnectionId.Id}\n";
		debugString += "---\n";

		debugString += $"LookInput={Inputs.LookInput.ToString()}\n";
		debugString += $"MoveInput={Inputs.MoveInput.ToString()}\n";
		ApplyDebugStringCustom(ref debugString);

		EditorDescription = debugString;
	}




#region VIRTUAL METHODS

	protected virtual void ApplyDebugStringCustom(ref string debugString)
	{
	}
	protected virtual void ProcessInputCustom(float deltaTimeF, ActorInput input)
	{
	}

	protected virtual void WriteStateTo(ObjectState state)
	{
	}

	protected virtual void ReadStateFrom(ObjectState state)
	{
	}

	protected virtual void SetInputCustom(ActorInput input)
	{
	}

	protected virtual void GetInputCustom(ActorInput input)
	{
	}

	protected virtual void HandleHealthChanged(HealthComponent.HealthUpdateInfo healthUpdateInfo)
	{

	}

#endregion
}
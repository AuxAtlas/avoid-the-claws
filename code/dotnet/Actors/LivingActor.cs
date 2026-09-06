#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Components;
using AvoidClaws.code.dotnet.Components.Core;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Actors;

public partial class LivingActor : CharacterBody3D, IActor
{
	[Signal]
	public delegate void RespawnTimerChangedEventHandler(float respawnTimerRemaining);

	[Inject]
	public CoreGame Core { get; } = null!;

#region EXPORTS

	[Export]
	protected Node3D ComponentContainer { get; private set; } = null!;

	[Export]
	protected Node3D BuffsContainer { get; private set; } = null!;

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
	public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
	public bool Destroyed => IsQueuedForDeletion();

	private readonly ObjectState[] _stateHistory = new ObjectState[NetworkManager.MaxTickSequence];
	protected ControllerInputs Inputs;

	protected readonly Dictionary<KableId, IComponent> Components = new();
	protected readonly List<IBuff> Buffs = new();

	public KableId KableId { get; private set; } = null!;

	public KableConnectionId AuthorityConnectionId { get; protected set; } = null!;
	public uint SpawnedOnTick { get; private set; }
	public bool ReconciliationMode { get; private set; }

	protected HealthComponent? HealthComponent;

	protected bool IsClient => Core.Network.IsClient;
	protected bool IsServer => Core.Network.IsServer;

	private StringBuilder _debugStringBuilder = new(100);

	private readonly ObjectState _stateCache = new();

	private readonly List<ObjectState> _reusableComponentStatesList = new();

	#endregion

	public void Spawned(uint spawnedTick)
	{
		SpawnedOnTick = spawnedTick;
	}
	
	public void KableSetup(KableId connectionId)
	{
		KableId = connectionId;
	}


	public virtual void Setup(uint tick)
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if(ComponentContainer == null)
			GD.PushError($"ComponentContainer is null on LivingActor with KableId '${KableId}'");
	}


	public virtual void Start(uint tick)
	{
		Components.Clear();
		foreach (var child in ComponentContainer.GetChildren())
		{
			if (child is IComponent component)
			{
				Components.Add(component.KableId, component);
			}
		}

		CacheComponentReferences();

		foreach (IComponent component in Components.Values)
		{
			component.Setup(tick);
			component.Start(tick);
		}
	}

	public virtual void Stop(uint tick)
	{
		foreach (IComponent component in Components.Values)
		{
			component.Stop(tick);
		}
	}

	public virtual void Teardown(uint tick)
	{
		foreach (IComponent component in Components.Values)
		{
			component.Teardown(tick);
		}
	}

	protected virtual void CacheComponentReferences()
	{
		if (HealthComponent is not null)
		{
			HealthComponent.Died -= HandleDiedEvent;
			HealthComponent.HealthChanged -= HandleHealthChanged;
		}

		foreach (var component in Components.Values)
			switch (component)
			{
				case HealthComponent healthComponent:
					HealthComponent = healthComponent;
					break;
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

		Vector3 spawnPos = new(-20f + Core.Random.NextSingle() * 40, 10f, -20f + Core.Random.NextSingle() * 40f);
		// Vector3 spawnPos = new(0f, 15f, 0f);
		TeleportTo(spawnPos);
	}


	public void SetInputs(in ControllerInputs inputs)
	{
		Inputs = inputs;
	}
	public T? GetComponent<T>() where T : IComponent
	{
		return (T?)Components.Values.FirstOrDefault(x => x is T);
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
		GetComponent<FpvCameraComponent>()?.TargetCamera?.SetCurrent(true);
	}

	public virtual void ProcessInput(uint processingTick)
	{
		foreach (IComponent component in Components.Values)
		{
			component.ProcessInput(Inputs, processingTick);
		}
		
		ProcessInputCustom(processingTick);
	}

	public void SetKableAuthority(KableConnectionId connectionId)
	{
		AuthorityConnectionId = connectionId;
	}

	public virtual void HandleNetTick(uint tick)
	{
		if (Destroyed || !Core.Network.FinishedInitialSync)
			return;

		// EditorDescription = GetDebugString();

		
		foreach (IComponent component in Components.Values)
		{
			component.HandleNetTick(tick);
		}
		
		ProcessInput(tick);

		if (IsServer)
		{
			_stateHistory[tick % NetworkManager.MaxTickSequence] = GetCurrentState(tick);
		}

		if (_respawnTimer > 0d && IsDead)
		{
			_respawnTimer -= TickDeltaTimeF;
			if (_respawnTimer <= 0f)
				Respawn();

			EmitSignalRespawnTimerChanged(_respawnTimer);
		}

		if (!IsDead)
		{
			MoveAndSlide();
		}
	}


	public void ApplyBuff(IBuff buff)
	{
		throw new NotImplementedException();
	}

	public void RemoveBuff(IBuff buff)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IBuff> GetBuffs()
	{
		return Buffs.AsEnumerable();
	}

	public ObjectState GetCurrentState(uint currentTick)
	{
		_stateCache.ResetCustoms();
		_stateCache.AuthorityConnectionId = AuthorityConnectionId;
		_stateCache.NetworkTick = currentTick;

		_stateCache.Put(Position);
		_stateCache.Put(Rotation);
		_stateCache.Put(Velocity);

		_stateCache.Put(Inputs.MoveInput);
		_stateCache.Put(Inputs.LookInput);
		_stateCache.Put(Inputs.AttackInputsPacked);
		_stateCache.Put(Inputs.ActionInputsPacked);

		GetCurrentStateCustom(_stateCache);

		return _stateCache;
	}

	public IReadOnlyCollection<ObjectState> GetComponentStates(uint currentTick)
	{
		_reusableComponentStatesList.Clear();
		foreach (IComponent c in Components.Values)
		{
			_reusableComponentStatesList.Add(c.GetCurrentState(currentTick));
		}

		return _reusableComponentStatesList.AsReadOnly();
	}

	public void SetCurrentState(in ObjectState state)
	{
		AuthorityConnectionId = state.AuthorityConnectionId;

		Position = state.ReadVector3();
		Rotation = state.ReadVector3();
		Velocity = state.ReadVector3();

		Inputs.MoveInput = state.ReadVector2();
		Inputs.LookInput = state.ReadVector2();
		Inputs.AttackInputsPacked = state.ReadByte();
		Inputs.ActionInputsPacked = state.ReadByte();

		SetCurrentStateCustom(state);
	}

	public void SetComponentStates(uint currentTick, in IReadOnlyCollection<ObjectState> states)
	{
		foreach (ObjectState componentState in states)
		{
			if (!Components.TryGetValue(componentState.ObjectId, out IComponent? targetComponent))
				continue;
			
			targetComponent.SetCurrentState(componentState);
		}
	}

	public void IngestNetworkState(in ObjectState state)
	{
		if (IsServer)
			throw new InvalidOperationException();

		_stateHistory[state.NetworkTick % NetworkManager.MaxTickSequence] = state;
	}

	public ObjectState GetHistoricState(uint targetTick)
	{
		return _stateHistory[targetTick % NetworkManager.MaxTickSequence];
	}

	public void RewindToTick(uint targetTick)
	{
		SetCurrentState(GetHistoricState(targetTick));
	}

	public void ResetInputs()
	{
		_moveInput = Vector2.Zero;
		_rotateInput = Vector2.Zero;
		_attackInputsPacked = 0;
		_actionInputsPacked = 0;
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

	public bool CheckNeedsNetReconciliation(ObjectState referenceState)
	{
		if (IsServer)
			return false;

		var referenceTick = referenceState.NetworkTick % NetworkManager.MaxTickSequence;
		var historicState = GetHistoricState(referenceTick);

		return Vector3.IsEqualApprox(referenceState.GetVector3AtIndex(0), historicState.GetVector3AtIndex(0), 0.001f);
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
		
		ResetPhysicsInterpolation();
	}


	public void TryPlaySound(AudioStream? soundEffect, float pitch = 1.0f, bool interrupt = false)
	{
		if (ReconciliationMode)
			return;

		if (soundEffect is null)
			return;

		GetComponent<AudioPlayerComponent>()?.Play(soundEffect, pitch, interrupt);
	}

	public bool IsLogicAuthority()
	{
		return AuthorityConnectionId == Core.Network.MyConnectionId;
	}

	public string GetDebugString()
	{
		_debugStringBuilder.Clear();
		_debugStringBuilder.Append("[DEBUG]\n");
		_debugStringBuilder.Append($"KableId={KableId.Id}\n");
		_debugStringBuilder.Append($"AuthorityConnId={AuthorityConnectionId.Id}\n");
		_debugStringBuilder.Append("---\n");
		_debugStringBuilder.Append($"LookInput={Inputs.LookInput.ToString()}\n");
		_debugStringBuilder.Append($"MoveInput={Inputs.MoveInput.ToString()}\n");

		GetDebugStringCustom(ref _debugStringBuilder);

		return _debugStringBuilder.ToString();
	}

#region EMPTY VIRTUAL METHODS

	protected virtual void GetDebugStringCustom(ref StringBuilder stringBuilder) { }

	protected virtual void HandleHealthChanged(HealthComponent.HealthUpdateInfo healthUpdateInfo) { }
	protected virtual void GetCurrentStateCustom(in ObjectState stateBuffer) { }
	protected virtual void SetCurrentStateCustom(in ObjectState objectState) { }
	protected virtual void ProcessInputCustom(uint processingTick) { }

	#endregion
}

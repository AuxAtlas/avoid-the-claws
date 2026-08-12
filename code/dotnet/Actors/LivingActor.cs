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
    public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
    public bool Destroyed => IsQueuedForDeletion();

    private readonly ObjectState[] _stateHistory = new ObjectState[NetworkManager.MaxTickSequence];
    protected ControllerInputs Inputs;

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

    private StringBuilder _debugStringBuilder = new(100);

    private readonly ObjectState _stateCache = new();

    #endregion

    public void KableSetup(KableId connectionId)
    {
        KableId = connectionId;
    }


    public void Setup()
    {
        if (ComponentContainer != null)
        {
            Components.Clear();
            foreach (var child in ComponentContainer.GetChildren())
                if (child is IComponent component)
                    Components.Add(component);

            CacheComponentReferences();

            Components.ForEach(x => x.SetupComponent());
        }
    }

    public void Start(uint startTick)
    {
        SpawnedOnTick = startTick;
    }

    public void Stop()
    {
    }

    public void Teardown()
    {
    }

    protected virtual void CacheComponentReferences()
    {
        if (HealthComponent is not null)
        {
            HealthComponent.Died -= HandleDiedEvent;
            HealthComponent.HealthChanged -= HandleHealthChanged;
        }

        foreach (var component in Components)
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

        Vector3 spawnPos = new(-300f + Core.Random.NextSingle() * 500f, 10f, -300f + Core.Random.NextSingle() * 500f);
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

        EditorDescription = GetDebugString();

        ProcessInput(tick);

        if (IsServer)
        {
            _stateHistory[tick % NetworkManager.MaxTickSequence] = GetCurrentState(tick);
        }

        // TODO: Add Client2Server input sending
        // if (NetworkManager.IsServer || AuthorityConnectionId.Equals(NetworkManager.MyConnectionId))
        // {
        //     SendInputSync(currentTick);
        // }

        if (_respawnTimer > 0d && IsDead)
        {
            _respawnTimer -= TickDeltaTimeF;
            if (_respawnTimer <= 0f) Respawn();

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

        return _stateCache;
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
        var motion = Velocity * deltaTimeF;
        var result = MoveAndCollide(motion);

        for (var i = 0; i < _maxMoveBounces; i++)
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

    protected virtual void GetDebugStringCustom(ref StringBuilder stringBuilder)
    {
    }

    protected virtual void ProcessInputCustom(float deltaTimeF, ControllerInputs input)
    {
    }

    protected virtual void SetInputCustom(ControllerInputs input)
    {
    }

    protected virtual void GetInputCustom(ControllerInputs input)
    {
    }

    protected virtual void HandleHealthChanged(HealthComponent.HealthUpdateInfo healthUpdateInfo)
    {
    }

    #endregion
}
using System;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Components;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.Buffs;

public abstract partial class BaseBuff : Node, IBuff
{
    [Inject]
    public CoreGame Core { get; } = null!;
    
    public KableId KableId { get; private set; }
    public uint SpawnedOnTick { get; private set; }
    public IActor? ParentActor { get; private set; }
    public bool IsProcessing { get; private set; }
    
    private bool _isDirty = false;
    public bool IsDirty => _isDirty || IsQueuedForDeletion();

    protected float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;

    protected bool ReconciliationMode => ParentActor?.ReconciliationMode ?? false;

    private readonly ObjectState _stateCache = new();
    
    private readonly ObjectState[] _stateHistory = new ObjectState[NetworkManager.MaxTickSequence];
    public KableConnectionId AuthorityConnectionId { get; private set; } = KableConnectionId.Server;

    
    public void KableSetup(KableId kableId)
    {
        KableId = kableId;
    }

    public virtual void Spawned(uint spawnedTick)
    {
        SpawnedOnTick = spawnedTick;
    }

    public virtual void Start(uint tick)
    {
        IsProcessing = true;
    }

    public virtual void Stop(uint tick)
    {
        IsProcessing = false;
    }
    public void OnAttachedHandler(IActor parentActor, uint tick)
    {
        ParentActor = parentActor;
        OnAttachedCustom(tick);
    }

    public void OnDetachedHandler(uint tick)
    {
        ParentActor = null;
        _isDirty = true;
        OnDetachedCustom(tick);
    }

    public ObjectState GetCurrentState(uint currentTick)
    {
        _stateCache.ResetCustoms();
        _stateCache.NetworkTick = currentTick;

        GetCurrentStateCustom(_stateCache);

        return _stateCache;
    }

    public void SetCurrentState(in ObjectState state)
    {
        SetCurrentStateCustom(state);
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

    public void SetKableAuthority(KableConnectionId connectionId)
    {
        AuthorityConnectionId = connectionId;
    }

#region OVERRIDABLE EMPTY METHODS
    
    protected virtual void GetCurrentStateCustom(in ObjectState stateBuffer) { }
    protected virtual void SetCurrentStateCustom(in ObjectState objectState) { }
    protected virtual void OnAttachedCustom(uint tick) { }
    protected virtual void OnDetachedCustom(uint tick) { }
    public virtual void HandleNetTick(uint tick) { }
    public virtual void ProcessInput(ControllerInputs inputs, uint tickToProcess) { }
    public virtual void Teardown(uint tick) { }

#endregion
}
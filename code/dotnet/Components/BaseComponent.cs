#region

using System;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Components;

public abstract partial class BaseComponent : Node3D, IComponent
{
    [Inject]
    public CoreGame Core { get; } = null!;
    
    public KableId KableId { get; private set; }
    public uint SpawnedOnTick { get; private set; }
    public IActor? ParentActor { get; private set; }
    
    public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;

    private readonly ObjectState _stateCache = new();
    
    private readonly ObjectState[] _stateHistory = new ObjectState[NetworkManager.MaxTickSequence];
    
    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;
    
    public bool Destroyed => IsQueuedForDeletion();

    public bool ReconciliationMode => ParentActor?.ReconciliationMode ?? false;
    public KableConnectionId AuthorityConnectionId { get; private set; } = KableConnectionId.Server;
    public void KableSetup(KableId kableId)
    {
        KableId = kableId;
    }

    public override void _Ready()
    {
        base._Ready();
        
        ParentActor = GetParent()?.GetParentOrNull<IActor>();
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
    public virtual void HandleNetTick(uint tick) { }
    public virtual void ProcessInput(ControllerInputs inputs, uint tickToProcess) { }
    public virtual void TeardownComponent() { }
    public virtual void Start(uint tick) { }
    public virtual void Setup(uint tick) { }
    public virtual void Spawned(uint spawnedTick) { }
    public virtual void Stop(uint tick) { }
    public virtual void Teardown(uint tick) { }

#endregion


}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.interfaces;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Controllers;

/// <summary>
/// Controller base class with always zero'd inputs
/// </summary>
public partial class BlankController : Node, IController, ILifecycleObject
{
    [Inject]
    public CoreGame Core { get; } = null!;

    private readonly List<IActor> _attachedActors = new();
    private readonly List<uint> _reusableUIntList = new();

    public KableId KableId { get; private set; }
    public KableConnectionId AuthorityConnectionId { get; private set; }
    public uint SpawnedOnTick { get; private set; }
    private byte _attackInputsPacked;
    private byte _actionInputsPacked;
    protected ObjectState[] _stateHistory = new ObjectState[NetworkManager.MaxTickSequence];
    protected ControllerInputs Inputs;

    public bool ReconciliationMode { get; private set; } = false;
    public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
    public bool Destroyed => IsQueuedForDeletion();
    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;

    private StringBuilder _debugStringBuilder = new(100);

    private readonly ObjectState _stateCache = new();
    
    

    public void Spawned(uint spawnedTick)
    {
        SpawnedOnTick = spawnedTick;
    }


    public void KableSetup(KableId kableId)
    {
        KableId = kableId;
        KableSetupCustom(kableId);
    }

    public void SetKableAuthority(KableConnectionId connectionId)
    {
        AuthorityConnectionId = connectionId;
    }

    public ObjectState GetCurrentState(uint currentTick)
    {
        _stateCache.ResetCustoms();
        _stateCache.NetworkTick = currentTick;
        _stateCache.ObjectId = KableId;
        _stateCache.AuthorityConnectionId = AuthorityConnectionId;

        _stateCache.Put(Inputs.MoveInput);
        _stateCache.Put(Inputs.LookInput);
        _stateCache.Put(Inputs.AttackInputsPacked);
        _stateCache.Put(Inputs.ActionInputsPacked);

        _stateCache.Put((uint)_attachedActors.Count);
        foreach (IActor actor in _attachedActors)
        {
            _stateCache.Put(actor.KableId.Id);
        }

        return _stateCache;
    }

    public void SetCurrentState(in ObjectState state)
    {
        AuthorityConnectionId = state.AuthorityConnectionId;

        Inputs.MoveInput = state.ReadVector2();
        Inputs.LookInput = state.ReadVector2();
        Inputs.AttackInputsPacked = state.ReadByte();
        Inputs.ActionInputsPacked = state.ReadByte();
        
        _reusableUIntList.Clear();
        uint attachedCount = state.ReadUInt();
        for (int i = 0; i < attachedCount; i++)
        {
            _reusableUIntList.Add(state.ReadUInt());
        }
        
        HashCode stateHash = new();
        HashCode currentHash = new();

        foreach (uint stateActorId in _reusableUIntList)
        {
            stateHash.Add(stateActorId);
        }

        foreach (IActor attachedActor in _attachedActors)
        {
            currentHash.Add(attachedActor.KableId.Id);
        }

        if (stateHash.ToHashCode() != currentHash.ToHashCode())
        {
            _attachedActors.Clear();
            foreach (uint actorToAttachId in _reusableUIntList)
            {
                IActor? foundActor = Core.World.Actors.GetActor(actorToAttachId);
                if (foundActor != null)
                    _attachedActors.Add(foundActor);
            }
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
        Inputs.MoveInput = Vector2.Zero;
        Inputs.LookInput = Vector2.Zero;
        _attackInputsPacked = 0;
        _actionInputsPacked = 0;
    }

    public void HandleNetTick(uint tick)
    {
        if (Destroyed || !Core.Network.FinishedInitialSync)
            return;

        EditorDescription = GetDebugString();

        HandleNetTickCustom(tick);
        
        _attachedActors.ForEach(x => x.SetInputs(Inputs));

        if (IsServer)
        {
            _stateHistory[tick % NetworkManager.MaxTickSequence] = GetCurrentState(tick);
        }
    }

    public virtual void Attach(IActor actor)
    {
        _attachedActors.Add(actor);
    }

    public virtual void Detach(IActor actor)
    {
        _attachedActors.Remove(actor);
    }

    public IReadOnlyList<IActor>? GetAttachments()
    {
        return _attachedActors.AsReadOnly();
    }


    protected void SetInputs(ControllerInputs inputs)
    {
        Inputs = inputs;
    }

    protected ref readonly ControllerInputs GetInputs()
    {
        return ref Inputs;
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
    protected virtual void KableSetupCustom(KableId kableId) { }
    protected virtual void HandleNetTickCustom(uint tick) { }
    protected virtual void GetDebugStringCustom(ref StringBuilder stringBuilder) { }

    public virtual void Setup(uint tick) { }
    public virtual void Start(uint tick) { }
    public virtual void Stop(uint tick) { }
    public virtual void Teardown(uint tick) { }

#endregion
}
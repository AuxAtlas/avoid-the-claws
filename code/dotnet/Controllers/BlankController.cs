#region

using System;
using System.Collections.Generic;
using System.Text;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Controllers;

/// <summary>
/// Controller base class with always zero'd inputs
/// </summary>
public partial class BlankController : Node, IController
{
    [Inject]
    public CoreGame Core { get; } = null!;

    private List<IActor> _attachedActors = new();

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

    public void KableSetup(KableId connectionId)
    {
        KableId = connectionId;
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

        return _stateCache;
    }

    public void SetCurrentState(ObjectState state)
    {
        AuthorityConnectionId = state.AuthorityConnectionId;

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
        Inputs.MoveInput = Vector2.Zero;
        Inputs.LookInput = Vector2.Zero;
        _attackInputsPacked = 0;
        _actionInputsPacked = 0;
    }

    public virtual void Setup()
    {
    }

    public void Start(uint startTick)
    {
        SpawnedOnTick = startTick;
    }

    public void HandleNetTick(uint tick)
    {
        if (Destroyed || !Core.Network.FinishedInitialSync)
            return;

        EditorDescription = GetDebugString();

        HandleNetTickCustom(tick);

        if (IsServer)
        {
            _stateHistory[tick % NetworkManager.MaxTickSequence] = GetCurrentState(tick);
        }
    }

    public void Stop()
    {
    }

    public void Teardown()
    {
    }

    public void Attach(IActor actor)
    {
    }

    public void Detach(IActor actor)
    {
    }

    public IReadOnlyList<IActor>? GetAttachments()
    {
        return _attachedActors.AsReadOnly();
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

    protected virtual void HandleNetTickCustom(uint tick)
    {
    }
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

#endregion
}
using System;
using System.Collections.Generic;
using System.Text;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

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
    protected ObjectState[] StateHistory = new ObjectState[NetworkManager.MAX_TICK_SEQUENCE];
    protected ActorInput Inputs;

    public bool ReconciliationMode { get; private set; } = false;
    public uint CurrentTick => Core.Network.NetworkTick;
    public float TickDeltaTimeF => NetworkManager.TickDeltaTimeF;
    public bool Destroyed => IsQueuedForDeletion();
    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;

    private StringBuilder _debugStringBuilder = new(100);
    
    public void KableSetup(KableId connectionId)
    {
        KableId = connectionId;
    }

    public void SetKableAuthority(KableConnectionId connectionId)
    {
        AuthorityConnectionId = connectionId;
    }

    public ObjectState GetCurrentState()
    {
        var state = new ObjectState
        {
            NetworkTick = CurrentTick,
            ObjectId = KableId,
            AuthorityConnectionId = AuthorityConnectionId
        };

        state.Put(Inputs.MoveInput);
        state.Put(Inputs.LookInput);
        state.Put(Inputs.AttackInputsPacked);
        state.Put(Inputs.ActionInputsPacked);

        return state;
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

        StateHistory[state.NetworkTick % NetworkManager.MAX_TICK_SEQUENCE] = state;
    }

    public void ResetInputs()
    {
        Inputs.MoveInput = Vector2.Zero;
        Inputs.LookInput = Vector2.Zero;
        _attackInputsPacked = 0;
        _actionInputsPacked = 0;
    }

    public virtual void Setup()
    { }

    public void Start()
    {
        SpawnedOnTick = CurrentTick;
    }

    public void HandleNetTick(uint tick)
    {
        if (Destroyed || !Core.Network.FinishedInitialSync)
            return;

        EditorDescription = GetDebugString();

        HandleNetTickCustom(tick);

        if (IsServer)
        {
            StateHistory[tick % NetworkManager.MAX_TICK_SEQUENCE] = GetCurrentState();
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

    protected virtual void ProcessInputCustom(float deltaTimeF, ActorInput input)
    {
    }

    protected virtual void SetInputCustom(ActorInput input)
    {
    }

    protected virtual void GetInputCustom(ActorInput input)
    {
    }
    
#endregion
}
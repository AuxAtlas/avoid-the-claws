#region

using System;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Components;

public abstract partial class BaseComponent : Node3D, IComponent
{
    public KableId KableId { get; private set; }
    public uint SpawnedOnTick { get; private set; }
    public IActor? ParentActor { get; private set; }

    public void KableSetup(KableId kableId)
    {
        KableId = kableId;
    }

    public virtual void SetupComponent()
    {
        ParentActor = GetParent()?.GetParentOrNull<IActor>();
    }
    public virtual void TeardownComponent()
    {
    }

    public abstract ObjectState GetCurrentState(uint currentTick);

    public abstract void SetCurrentState(in ObjectState state);
    public virtual void IngestNetworkState(in ObjectState state)
    {
    }

    public virtual ObjectState GetHistoricState(uint targetTick)
    {
        return ObjectState.BlankStateRef;
    }

    public virtual void RewindToTick(uint targetTick)
    {
    }

    public virtual void HandleReconciliationUntilTick(uint startTick, uint endTick)
    {
    }
    public virtual void HandleNetTick(uint tick)
    {
    }
    public void ProcessInput(ControllerInputs inputs, uint tickToProcess)
    {
        throw new NotImplementedException();
    }
}
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

    public virtual ObjectState GetCurrentState(uint currentTick)
    {
        throw new NotImplementedException();
    }

    public virtual void SetCurrentState(ObjectState state)
    {
        throw new NotImplementedException();
    }

    public virtual void IngestNetworkState(ObjectState state)
    {
        throw new NotImplementedException();
    }

    public virtual ObjectState GetHistoricState(uint targetTick)
    {
        throw new NotImplementedException();
    }

    public virtual void RewindToTick(uint targetTick)
    {
        throw new NotImplementedException();
    }

    public virtual void HandleReconciliationUntilTick(uint startTick, uint endTick)
    {
        throw new NotImplementedException();
    }
    public virtual void HandleNetTick(uint tick)
    {
        throw new NotImplementedException();
    }
}
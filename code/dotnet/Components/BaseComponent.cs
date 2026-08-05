using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;

namespace AvoidClaws.code.dotnet.Components;

public partial class BaseComponent : Node3D, IComponent
{
    public KableId KableId { get; private set;  }
    public uint SpawnedOnTick { get; private set; }
    
    public bool ReconciliationMode => ParentActor?.ReconciliationMode ?? false;
    public IActor? ParentActor { get; private set; }
    
    public void KableSetup(KableId kableId)
    {
        KableId = kableId;
    }

    public virtual void ReadStateFrom(ref ObjectState state)
    {
        
    }

    public virtual void WriteStateTo(ref ObjectState state)
    {
        
    }

    public virtual void SetupComponent()
    {
        ParentActor = GetParent()?.GetParentOrNull<IActor>();
    }
    public virtual void TeardownComponent()
    { }
}
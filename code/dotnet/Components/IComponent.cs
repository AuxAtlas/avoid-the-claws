using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;

namespace AvoidClaws.code.dotnet.Components;

public interface IComponent
{
    public bool ReconciliationMode { get; }
    public IActor? ParentActor { get; }

    public void SetupComponent();

    public void ReadStateFrom(ObjectState state);
    public void WriteStateTo(ObjectState state);
}
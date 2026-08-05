using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;

namespace AvoidClaws.code.dotnet.Components;

public interface IComponent : IStateObject
{
    public bool ReconciliationMode { get; }
    public IActor? ParentActor { get; }

    public void SetupComponent();
}
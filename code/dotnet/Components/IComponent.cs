#region

using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;

#endregion

namespace AvoidClaws.code.dotnet.Components;

public interface IComponent : IStateObject
{
    public IActor? ParentActor { get; }

    public void SetupComponent();
}
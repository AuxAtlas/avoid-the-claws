#region

using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue;
using AvoidClaws.code.dotnet.Glue.interfaces;

#endregion

namespace AvoidClaws.code.dotnet.Components;

public interface IComponent : IGameObject
{
    public IActor? ParentActor { get; }

    public void ProcessInput(ControllerInputs inputs, uint processingTick);
}
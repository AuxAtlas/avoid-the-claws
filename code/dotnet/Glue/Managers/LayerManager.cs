#region

using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Glue.Managers;

public partial class LayerManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    [Export]
    public CanvasLayer PauseLayer = null!;
}
using AvoidClaws.code.dotnet.Events;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Levels;
using AvoidClaws.code.dotnet.Resources;
using Godot;

namespace AvoidClaws.code.dotnet.Services;

public partial class CoreGame : Node, IService
{
    [Inject]
    public EventBus EventBus { get; } = null!;

    [Inject]
    public NetworkManager Network { get; } = null!;

    [Inject]
    public GameResources Resources { get; } = null!;

    [Inject]
    public GameWorld World { get; } = null!;
}
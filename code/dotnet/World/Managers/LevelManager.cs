using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class LevelManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;
}
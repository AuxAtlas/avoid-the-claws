using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class LevelManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    public void ChangeMapTo(PackedScene map)
    {
        Core.World.ResetWorld();
        foreach (var child in GetChildren())
            child.QueueFree();

        var mapNode = map.Instantiate();
        if (mapNode is null)
            return;

        AddChild(mapNode);
    }
}